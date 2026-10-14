namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run bundle is in its life cycle.</summary>
public enum PayRunBundleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a pay run bundle and its current status.</summary>
public sealed class PayRunBundle(string code, string title, int createdAt, double budget, string location, PayRunBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public double Budget { get; set; } = budget;
    public string Location { get; set; } = location;
    public PayRunBundleStatus Status { get; set; } = status;

    /// <summary>Starts a pay run bundle in status proposed.</summary>
    public static PayRunBundle NewPayRunBundle(string code, string title)
    {
        return new PayRunBundle(code, title, 0, 0.0, "", PayRunBundleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunBundleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunBundleStatus next)
    {
        switch (Status)
        {
            case PayRunBundleStatus.Proposed:
                switch (next)
                {
                    case PayRunBundleStatus.Rejected:
                    case PayRunBundleStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayRunBundleStatus.Approved:
                switch (next)
                {
                    case PayRunBundleStatus.Rejected:
                    case PayRunBundleStatus.Withdrawn:
                        return true;
                }
                return false;
            case PayRunBundleStatus.Rejected:
                switch (next)
                {
                    case PayRunBundleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedVolume()
    {
        return Math.Round(Budget * (double)CreatedAt, 2);
    }

    public PayRunBundle Copy()
    {
        return new PayRunBundle(Code, Title, CreatedAt, Budget, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
