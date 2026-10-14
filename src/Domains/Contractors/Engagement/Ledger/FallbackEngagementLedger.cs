namespace Acme.Workforce.Contractors.Engagement.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback engagement ledger is in its life cycle.</summary>
public enum FallbackEngagementLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A fallback engagement ledger. Its code is unique within a tenant.</summary>
public sealed class FallbackEngagementLedger(string code, string title, int windowDays, string channel, double budget, Dictionary<string, string> attributes, string jobCode, bool visible, int version, string segment, string salaryBand, FallbackEngagementLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string JobCode { get; set; } = jobCode;
    public bool Visible { get; set; } = visible;
    public int Version { get; set; } = version;
    public string Segment { get; set; } = segment;
    public string SalaryBand { get; set; } = salaryBand;
    public FallbackEngagementLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a fallback engagement ledger in status created.</summary>
    public static FallbackEngagementLedger NewFallbackEngagementLedger(string code, string title)
    {
        return new FallbackEngagementLedger(
            code,
            title,
            0,
            "",
            0.0,
            new Dictionary<string, string>(),
            "",
            false,
            0,
            "",
            "",
            FallbackEngagementLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackEngagementLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackEngagementLedgerStatus next)
    {
        switch (Status)
        {
            case FallbackEngagementLedgerStatus.Created:
                return next == FallbackEngagementLedgerStatus.Validated || next == FallbackEngagementLedgerStatus.Published;
            case FallbackEngagementLedgerStatus.Validated:
                switch (next)
                {
                    case FallbackEngagementLedgerStatus.Retired:
                        return true;
                }
                return false;
            case FallbackEngagementLedgerStatus.Published:
                return next == FallbackEngagementLedgerStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FallbackEngagementLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback engagement ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"fallback engagement ledger"} {Code}";
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

    public double EffectiveVolume()
    {
        return Math.Round((double)Version + (double)WindowDays, 2);
    }

    public FallbackEngagementLedger Copy()
    {
        return new FallbackEngagementLedger(
            Code,
            Title,
            WindowDays,
            Channel,
            Budget,
            Attributes,
            JobCode,
            Visible,
            Version,
            Segment,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
