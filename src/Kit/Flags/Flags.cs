namespace Acme.Workforce.Kit.Flags;

using System.Security.Cryptography;
using System.Text;

/// <summary>Strategy decides whether a flag is on for a subject.</summary>
public interface IStrategy
{
    bool Allows(string subject);
}

public sealed record PercentStrategy(int Rollout, string Salt) : IStrategy
{
    public bool Allows(string subject)
    {
        var bucket = (int)(BitConverter.ToUInt32(SHA256.HashData(Encoding.UTF8.GetBytes(Salt + subject))) & 0x7fffffff) % 100;
        return bucket < Rollout;
    }
}

public sealed record AllowListStrategy(List<string> Subjects) : IStrategy
{
    public bool Allows(string subject)
    {
        return Subjects.Contains(subject);
    }
}

public sealed record PrefixStrategy(string Prefix) : IStrategy
{
    public bool Allows(string subject)
    {
        return subject.StartsWith(Prefix, StringComparison.Ordinal);
    }
}

public sealed record AllOf(List<IStrategy> Parts) : IStrategy
{
    public bool Allows(string subject)
    {
        foreach (var part in Parts)
        {
            if (!part.Allows(subject))
            {
                return false;
            }
        }
        return true;
    }
}

public sealed record AnyOf(List<IStrategy> Parts) : IStrategy
{
    public bool Allows(string subject)
    {
        foreach (var part in Parts)
        {
            if (part.Allows(subject))
            {
                return true;
            }
        }
        return false;
    }
}

public sealed record Flag(string Name, bool Enabled, IStrategy Strategy);

/// <summary>Registry holds the flags of a process by name.</summary>
public sealed record Registry(Dictionary<string, Flag> Flags)
{
    public void Register(Flag flag)
    {
        Flags[flag.Name] = flag;
    }

    public bool Enabled(string name, string subject)
    {
        if (!Flags.TryGetValue(name, out var found))
        {
            return false;
        }
        if (!found.Enabled)
        {
            return false;
        }
        return found.Strategy.Allows(subject);
    }

    public List<string> Names()
    {
        return Flags.Keys.Order(StringComparer.Ordinal).ToList();
    }
}

/// <summary>Functions of flags.</summary>
public static class Flags
{
    public static Registry NewRegistry()
    {
        return new Registry(new Dictionary<string, Flag>());
    }
}
