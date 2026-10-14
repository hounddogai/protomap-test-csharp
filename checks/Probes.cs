using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Acme.Workforce.Platform;
using Dapper;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Microsoft.Extensions.Configuration;

namespace Acme.Workforce.Checks;

/// <summary>Captures real generated client calls without contacting Acme's other repositories.</summary>
public sealed class ProbeInvoker : CallInvoker
{
    public ConcurrentQueue<(string Method, IMessage Request, IMessage Response)> Calls { get; } = new();
    public ConcurrentQueue<(string Method, Metadata? Headers)> Sent { get; } = new();
    /// <summary>Answers a call by its method and request instead of probe data, when it gives an answer.</summary>
    public Func<string, IMessage, IMessage?>? Answer { get; set; }

    public void Clear()
    {
        Calls.Clear();
        Sent.Clear();
    }

    private TResponse Capture<TRequest, TResponse>(Method<TRequest, TResponse> method, CallOptions options,
        TRequest request) where TRequest : class where TResponse : class
    {
        var fqn = method.FullName.TrimStart('/');
        var answered = Answer?.Invoke(fqn, (IMessage)request) as TResponse;
        var response = answered ?? Activator.CreateInstance<TResponse>();
        if (answered is null)
            ProbeData.Fill((IMessage)response, method.FullName, 0);
        Calls.Enqueue((fqn, (IMessage)request, (IMessage)response));
        Sent.Enqueue((fqn, options.Headers));
        return response;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method,
        string? host, CallOptions options, TRequest request) => new(
            Task.FromResult(Capture(method, options, request)), Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => new Metadata(), () => { });

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method,
        string? host, CallOptions options, TRequest request) => Capture(method, options, request);

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request) => new(
            new ProbeReader<TResponse>(new[] { Capture(method, options, request) }), Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => new Metadata(), () => { });

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options)
    {
        var completion = new TaskCompletionSource<TResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        var writer = new ProbeWriter<TRequest>(request => completion.TrySetResult(Capture(method, options, request)));
        return new(writer, completion.Task, Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => new Metadata(), () => { });
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        Method<TRequest, TResponse> method, string? host, CallOptions options)
    {
        var channel = System.Threading.Channels.Channel.CreateUnbounded<TResponse>();
        var writer = new ProbeWriter<TRequest>(request => channel.Writer.TryWrite(Capture(method, options, request)),
            () => channel.Writer.TryComplete());
        return new(writer, new ChannelReader<TResponse>(channel.Reader), Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess, () => new Metadata(), () => { });
    }
}

public sealed class ProbeReader<T>(IEnumerable<T> items) : IAsyncStreamReader<T> where T : class
{
    private readonly IEnumerator<T> _enumerator = items.GetEnumerator();
    public T Current => _enumerator.Current;
    public Task<bool> MoveNext(CancellationToken token) => Task.FromResult(_enumerator.MoveNext());
}

public sealed class ChannelReader<T>(System.Threading.Channels.ChannelReader<T> reader) : IAsyncStreamReader<T>
    where T : class
{
    public T Current { get; private set; } = null!;
    public async Task<bool> MoveNext(CancellationToken token)
    {
        if (!await reader.WaitToReadAsync(token))
            return false;
        Current = await reader.ReadAsync(token);
        return true;
    }
}

public sealed class ProbeWriter<T>(Action<T>? onWrite = null, Action? onComplete = null) :
    IServerStreamWriter<T>, IClientStreamWriter<T> where T : class
{
    public List<T> Messages { get; } = [];
    public WriteOptions? WriteOptions { get; set; }
    public Task WriteAsync(T message)
    {
        Messages.Add(message);
        onWrite?.Invoke(message);
        return Task.CompletedTask;
    }
    public Task CompleteAsync()
    {
        onComplete?.Invoke();
        return Task.CompletedTask;
    }
}

public sealed class ProbeContext : ServerCallContext
{
    protected override string MethodCore => "/checks/probe";
    protected override string HostCore => "localhost";
    protected override string PeerCore => "checks";
    protected override DateTime DeadlineCore => DateTime.MaxValue;
    protected override Metadata RequestHeadersCore { get; } = new();
    protected override CancellationToken CancellationTokenCore => CancellationToken.None;
    protected override Metadata ResponseTrailersCore { get; } = new();
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore => new("checks", new Dictionary<string, List<AuthProperty>>());
    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) =>
        throw new NotSupportedException();
    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
}

public sealed class NullRepository : IRecordRepository
{
    public Task SaveAsync(StoredRecord record, CancellationToken token) => Task.CompletedTask;
    public Task RecordAsync(string tenant, string kind, string id, string action, string detail, CancellationToken token) =>
        Task.CompletedTask;
}

public static class ProbeData
{
    public static void Fill(IMessage message, string seed, int variant, int depth = 0)
    {
        if (depth > 5 || message.Descriptor.FullName.StartsWith("google.protobuf.", StringComparison.Ordinal))
            return;
        foreach (var field in message.Descriptor.Fields.InFieldNumberOrder())
        {
            if (field.ContainingOneof is { } group && group.Fields[variant % group.Fields.Count] != field)
                continue;
            var valueSeed = seed + "." + field.Name;
            if (field.IsMap)
            {
                var valueField = field.MessageType.FindFieldByNumber(2);
                ((IDictionary)field.Accessor.GetValue(message)).Add("primary", Value(valueField, valueSeed, variant, depth));
            }
            else if (field.IsRepeated)
            {
                var list = (IList)field.Accessor.GetValue(message);
                list.Add(Value(field, valueSeed + "/0", variant, depth));
                list.Add(Value(field, valueSeed + "/1", variant, depth));
            }
            else
                field.Accessor.SetValue(message, Value(field, valueSeed, variant, depth));
        }
    }

    private static object Value(FieldDescriptor field, string seed, int variant, int depth)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        if (field.FieldType == FieldType.Message)
        {
            var child = (IMessage)(Activator.CreateInstance(field.MessageType.ClrType)
                ?? throw new InvalidOperationException("Cannot create a message"));
            Fill(child, seed, variant, depth + 1);
            return child;
        }
        return field.FieldType switch
        {
            FieldType.String => "v" + Convert.ToHexString(hash.AsSpan(0, 8)),
            FieldType.Bytes => ByteString.CopyFrom(hash),
            FieldType.Bool => true,
            FieldType.Int32 => (int)hash[0] + 1,
            FieldType.Int64 => (long)hash[0] + 1,
            FieldType.Double => (double)hash[0] + 0.5,
            FieldType.Enum => Enum.ToObject(field.EnumType.ClrType, field.EnumType.Values[1].Number),
            _ => throw new InvalidOperationException("Unsupported probe scalar: " + field.FieldType)
        };
    }

    public static int CheckFlows(JsonElement oracle, string handler, IMessage request, IMessage[] responses,
        ProbeInvoker invoker, string[] conditionalCalls)
    {
        var count = 0;
        foreach (var call in oracle.GetProperty("calls").EnumerateArray())
        {
            if (call.GetProperty("from").TryGetProperty("handler", out var caller) && caller.GetString() == handler
                && call.GetProperty("pattern").GetString() != "server_interceptor"
                && !conditionalCalls.Contains(call.GetProperty("to").GetString())
                && !invoker.Calls.Any(actual => actual.Method == call.GetProperty("to").GetString()))
                throw new InvalidOperationException($"{handler}: planned RPC {call.GetProperty("to")} was not called");
        }
        foreach (var flow in oracle.GetProperty("flows").EnumerateArray())
        {
            if (!flow.GetProperty("island").TryGetProperty("handler", out var island) || island.GetString() != handler
                || flow.GetProperty("status").GetString() != "required")
                continue;
            if (flow.TryGetProperty("transform", out var transform) && transform.GetString() is "any_pack" or "json_bytes")
                continue;
            var from = flow.GetProperty("source");
            var to = flow.GetProperty("target");
            if (to.GetProperty("kind").GetString() == "call_request"
                && conditionalCalls.Contains(to.GetProperty("method").GetString())
                && !invoker.Calls.Any(call => call.Method == to.GetProperty("method").GetString()))
                continue;
            var inputs = from.GetProperty("kind").GetString() == "handler_request" ? new[] { request } :
                invoker.Calls.Where(call => call.Method == from.GetProperty("method").GetString()).Select(call => call.Response);
            var outputs = to.GetProperty("kind").GetString() == "handler_response" ? responses :
                invoker.Calls.Where(call => call.Method == to.GetProperty("method").GetString()).Select(call => call.Request);
            var expected = inputs.SelectMany(input => Read(input, from.GetProperty("path"))).ToArray();
            var actual = outputs.SelectMany(output => Read(output, to.GetProperty("path"))).ToHashSet(StringComparer.Ordinal);
            foreach (var value in expected)
            {
                if (value.Length == 0)
                    continue;
                if (!actual.Contains(value))
                    throw new InvalidOperationException($"{handler}: {flow.GetProperty("id")} lost {value}");
                count++;
            }
        }
        return count;
    }

    private static IEnumerable<string> Read(IMessage message, JsonElement path)
    {
        IEnumerable<object> values = new object[] { message };
        foreach (var segment in path.EnumerateArray())
        {
            var name = segment.GetString()!.Split('.').Last();
            values = values.OfType<IMessage>().SelectMany(value =>
            {
                var field = value.Descriptor.FindFieldByName(name);
                if (field.ContainingOneof is { } oneof && oneof.Accessor.GetCaseFieldDescriptor(value) != field)
                    return Array.Empty<object>();
                var child = field.Accessor.GetValue(value);
                if (field.IsMap)
                {
                    var map = (IDictionary)child;
                    return map.Contains("primary") ? new object[] { map["primary"]! } : Array.Empty<object>();
                }
                if (field.IsRepeated)
                    return ((IList)child).Cast<object>();
                return child is null ? Array.Empty<object>() : new[] { child };
            }).ToArray();
        }
        return values.Select(value => value is IMessage proto ? Convert.ToBase64String(proto.ToByteArray()) :
            Convert.ToString(value, CultureInfo.InvariantCulture) ?? "");
    }

    public static async Task CheckDatabaseAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), "acme-check-" + Guid.NewGuid() + ".db");
        try
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Workforce"] = "Data Source=" + path + ";Pooling=False"
            }).Build();
            var database = new WorkforceDatabase(configuration);
            await database.InitializeAsync(CancellationToken.None);
            var repository = new DapperRecordRepository(database);
            await repository.SaveAsync(new StoredRecord("test", "employee", "42", new byte[] { 1, 2 }), CancellationToken.None);
            await repository.SaveAsync(new StoredRecord("test", "employee", "42", new byte[] { 3, 4 }), CancellationToken.None);
            await repository.RecordAsync("test", "employee", "42", "update", "local detail", CancellationToken.None);
            await using var connection = await database.OpenAsync(CancellationToken.None);
            var revision = await connection.QuerySingleAsync<long>("SELECT revision FROM records WHERE id = '42'");
            var actions = await connection.QuerySingleAsync<long>("SELECT count(*) FROM actions");
            if (revision != 2 || actions != 1)
                throw new InvalidOperationException("SQLite snapshots or audit writes were lost");
        }
        finally
        {
            File.Delete(path);
        }
    }
}
