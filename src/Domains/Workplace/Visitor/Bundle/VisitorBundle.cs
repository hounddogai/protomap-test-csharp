namespace Acme.Workforce.Workplace.Visitor.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor bundle is in its life cycle.</summary>
public enum VisitorBundleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a visitor bundle and its current status.</summary>
public sealed class VisitorBundle(string code, string name, Dictionary<string, string> attributes, int updatedAt, string title, double rate, double minimum, string employeeId, double score, List<string> tags, VisitorBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public double Minimum { get; set; } = minimum;
    public string EmployeeId { get; set; } = employeeId;
    public double Score { get; set; } = score;
    public List<string> Tags { get; set; } = tags;
    public VisitorBundleStatus Status { get; set; } = status;

    /// <summary>Starts a visitor bundle in status requested.</summary>
    public static VisitorBundle NewVisitorBundle(string code, string name)
    {
        return new VisitorBundle(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            "",
            0.0,
            0.0,
            "",
            0.0,
            new List<string>(),
            VisitorBundleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorBundleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorBundleStatus next)
    {
        switch (Status)
        {
            case VisitorBundleStatus.Requested:
                return next == VisitorBundleStatus.Granted;
            case VisitorBundleStatus.Granted:
                switch (next)
                {
                    case VisitorBundleStatus.Revoked:
                        return true;
                }
                return false;
            case VisitorBundleStatus.Revoked:
                switch (next)
                {
                    case VisitorBundleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
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

    public double NetValue()
    {
        return Math.Round((double)UpdatedAt * Score, 2);
    }

    public VisitorBundle Copy()
    {
        return new VisitorBundle(
            Code,
            Name,
            Attributes,
            UpdatedAt,
            Title,
            Rate,
            Minimum,
            EmployeeId,
            Score,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
