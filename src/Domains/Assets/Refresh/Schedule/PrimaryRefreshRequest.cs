namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary refresh request is in its life cycle.</summary>
public enum PrimaryRefreshRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a primary refresh request and its current status.</summary>
public sealed class PrimaryRefreshRequest(string code, string name, Dictionary<string, string> attributes, string badge, string source, string location, double weight, int capacity, PrimaryRefreshRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Badge { get; set; } = badge;
    public string Source { get; set; } = source;
    public string Location { get; set; } = location;
    public double Weight { get; set; } = weight;
    public int Capacity { get; set; } = capacity;
    public PrimaryRefreshRequestStatus Status { get; set; } = status;

    /// <summary>Starts a primary refresh request in status created.</summary>
    public static PrimaryRefreshRequest NewPrimaryRefreshRequest(string code, string name)
    {
        return new PrimaryRefreshRequest(
            code,
            name,
            new Dictionary<string, string>(),
            "",
            "",
            "",
            0.0,
            0,
            PrimaryRefreshRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryRefreshRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryRefreshRequestStatus next)
    {
        switch (Status)
        {
            case PrimaryRefreshRequestStatus.Created:
                return next == PrimaryRefreshRequestStatus.Published;
            case PrimaryRefreshRequestStatus.Validated:
                return next == PrimaryRefreshRequestStatus.Published || next == PrimaryRefreshRequestStatus.Retired;
            case PrimaryRefreshRequestStatus.Published:
                return next == PrimaryRefreshRequestStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PrimaryRefreshRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary refresh request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary refresh request"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedLoad()
    {
        return Math.Round(Weight + (double)Capacity, 2);
    }

    public PrimaryRefreshRequest Copy()
    {
        return new PrimaryRefreshRequest(Code, Name, Attributes, Badge, Source, Location, Weight, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
