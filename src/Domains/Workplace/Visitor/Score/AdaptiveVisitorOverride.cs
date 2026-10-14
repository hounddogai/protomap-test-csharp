namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive visitor override is in its life cycle.</summary>
public enum AdaptiveVisitorOverrideStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A adaptive visitor override. Its code is unique within a tenant.</summary>
public sealed class AdaptiveVisitorOverride(string code, string name, double margin, string category, string notes, string externalRef, double ceiling, string checksum, int batchSize, string currency, string segment, AdaptiveVisitorOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public string Category { get; set; } = category;
    public string Notes { get; set; } = notes;
    public string ExternalRef { get; set; } = externalRef;
    public double Ceiling { get; set; } = ceiling;
    public string Checksum { get; set; } = checksum;
    public int BatchSize { get; set; } = batchSize;
    public string Currency { get; set; } = currency;
    public string Segment { get; set; } = segment;
    public AdaptiveVisitorOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive visitor override in status requested.</summary>
    public static AdaptiveVisitorOverride NewAdaptiveVisitorOverride(string code, string name)
    {
        return new AdaptiveVisitorOverride(
            code,
            name,
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            0,
            "",
            "",
            AdaptiveVisitorOverrideStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveVisitorOverrideStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveVisitorOverrideStatus next)
    {
        switch (Status)
        {
            case AdaptiveVisitorOverrideStatus.Requested:
                switch (next)
                {
                    case AdaptiveVisitorOverrideStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveVisitorOverrideStatus.Granted:
                return next == AdaptiveVisitorOverrideStatus.Expired;
            case AdaptiveVisitorOverrideStatus.Revoked:
                switch (next)
                {
                    case AdaptiveVisitorOverrideStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveVisitorOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive visitor override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Ceiling + (double)BatchSize, 2);
    }

    public AdaptiveVisitorOverride Copy()
    {
        return new AdaptiveVisitorOverride(
            Code,
            Name,
            Margin,
            Category,
            Notes,
            ExternalRef,
            Ceiling,
            Checksum,
            BatchSize,
            Currency,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
