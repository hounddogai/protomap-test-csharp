namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient mobility slot is in its life cycle.</summary>
public enum LenientMobilitySlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One lenient mobility slot, with what the planners need to act on it.</summary>
public sealed class LenientMobilitySlot(string code, string title, double minimum, string department, double fte, int batchSize, double rate, Dictionary<string, string> attributes, double margin, string jobCode, int expiresAt, LenientMobilitySlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Department { get; set; } = department;
    public double Fte { get; set; } = fte;
    public int BatchSize { get; set; } = batchSize;
    public double Rate { get; set; } = rate;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Margin { get; set; } = margin;
    public string JobCode { get; set; } = jobCode;
    public int ExpiresAt { get; set; } = expiresAt;
    public LenientMobilitySlotStatus Status { get; set; } = status;

    /// <summary>Starts a lenient mobility slot in status requested.</summary>
    public static LenientMobilitySlot NewLenientMobilitySlot(string code, string title)
    {
        return new LenientMobilitySlot(
            code,
            title,
            0.0,
            "",
            0.0,
            0,
            0.0,
            new Dictionary<string, string>(),
            0.0,
            "",
            0,
            LenientMobilitySlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientMobilitySlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientMobilitySlotStatus next)
    {
        switch (Status)
        {
            case LenientMobilitySlotStatus.Requested:
                switch (next)
                {
                    case LenientMobilitySlotStatus.Revoked:
                    case LenientMobilitySlotStatus.Expired:
                        return true;
                }
                return false;
            case LenientMobilitySlotStatus.Granted:
                switch (next)
                {
                    case LenientMobilitySlotStatus.Revoked:
                    case LenientMobilitySlotStatus.Expired:
                        return true;
                }
                return false;
            case LenientMobilitySlotStatus.Revoked:
                return next == LenientMobilitySlotStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LenientMobilitySlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient mobility slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double NetExposure()
    {
        return Math.Round((double)BatchSize - Margin, 2);
    }

    public LenientMobilitySlot Copy()
    {
        return new LenientMobilitySlot(
            Code,
            Title,
            Minimum,
            Department,
            Fte,
            BatchSize,
            Rate,
            Attributes,
            Margin,
            JobCode,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
