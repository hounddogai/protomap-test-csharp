namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay bundle is in its life cycle.</summary>
public enum RetroPayBundleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a retro pay bundle and its current status.</summary>
public sealed class RetroPayBundle(string code, string label, bool active, Dictionary<string, string> attributes, double score, double minimum, int updatedAt, int tenureMonths, int revision, double floorValue, List<string> tags, RetroPayBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Score { get; set; } = score;
    public double Minimum { get; set; } = minimum;
    public int UpdatedAt { get; set; } = updatedAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Revision { get; set; } = revision;
    public double FloorValue { get; set; } = floorValue;
    public List<string> Tags { get; set; } = tags;
    public RetroPayBundleStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay bundle in status draft.</summary>
    public static RetroPayBundle NewRetroPayBundle(string code, string label)
    {
        return new RetroPayBundle(
            code,
            label,
            false,
            new Dictionary<string, string>(),
            0.0,
            0.0,
            0,
            0,
            0,
            0.0,
            new List<string>(),
            RetroPayBundleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayBundleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayBundleStatus next)
    {
        switch (Status)
        {
            case RetroPayBundleStatus.Draft:
                switch (next)
                {
                    case RetroPayBundleStatus.Pending:
                        return true;
                }
                return false;
            case RetroPayBundleStatus.Pending:
                switch (next)
                {
                    case RetroPayBundleStatus.Suspended:
                    case RetroPayBundleStatus.Closed:
                        return true;
                }
                return false;
            case RetroPayBundleStatus.Active:
                return next == RetroPayBundleStatus.Suspended;
            case RetroPayBundleStatus.Suspended:
                return next == RetroPayBundleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RetroPayBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double EffectiveTotal()
    {
        return Math.Round((double)UpdatedAt - (double)TenureMonths, 2);
    }

    public RetroPayBundle Copy()
    {
        return new RetroPayBundle(
            Code,
            Label,
            Active,
            Attributes,
            Score,
            Minimum,
            UpdatedAt,
            TenureMonths,
            Revision,
            FloorValue,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
