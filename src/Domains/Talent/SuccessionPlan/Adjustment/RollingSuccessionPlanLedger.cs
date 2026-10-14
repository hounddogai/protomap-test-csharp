namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling succession plan ledger is in its life cycle.</summary>
public enum RollingSuccessionPlanLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A rolling succession plan ledger as the succession plan team keeps it.</summary>
public sealed class RollingSuccessionPlanLedger(string code, string label, string externalRef, string jobCode, string channel, int capacity, RollingSuccessionPlanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string JobCode { get; set; } = jobCode;
    public string Channel { get; set; } = channel;
    public int Capacity { get; set; } = capacity;
    public RollingSuccessionPlanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a rolling succession plan ledger in status created.</summary>
    public static RollingSuccessionPlanLedger NewRollingSuccessionPlanLedger(string code, string label)
    {
        return new RollingSuccessionPlanLedger(
            code,
            label,
            "",
            "",
            "",
            0,
            RollingSuccessionPlanLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSuccessionPlanLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSuccessionPlanLedgerStatus next)
    {
        switch (Status)
        {
            case RollingSuccessionPlanLedgerStatus.Created:
                switch (next)
                {
                    case RollingSuccessionPlanLedgerStatus.Published:
                    case RollingSuccessionPlanLedgerStatus.Retired:
                        return true;
                }
                return false;
            case RollingSuccessionPlanLedgerStatus.Validated:
                return next == RollingSuccessionPlanLedgerStatus.Retired;
            case RollingSuccessionPlanLedgerStatus.Published:
                switch (next)
                {
                    case RollingSuccessionPlanLedgerStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingSuccessionPlanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling succession plan ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling succession plan ledger"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Capacity;
    }

    public RollingSuccessionPlanLedger Copy()
    {
        return new RollingSuccessionPlanLedger(Code, Label, ExternalRef, JobCode, Channel, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
