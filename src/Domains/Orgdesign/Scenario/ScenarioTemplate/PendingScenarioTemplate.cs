namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending scenario template is in its life cycle.</summary>
public enum PendingScenarioTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending scenario template as the scenario team keeps it.</summary>
public sealed class PendingScenarioTemplate(string code, string name, string unit, double ceiling, string salaryBand, string checksum, int durationMinutes, int endsAt, string currency, double floorValue, string ownerId, PendingScenarioTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public string SalaryBand { get; set; } = salaryBand;
    public string Checksum { get; set; } = checksum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int EndsAt { get; set; } = endsAt;
    public string Currency { get; set; } = currency;
    public double FloorValue { get; set; } = floorValue;
    public string OwnerId { get; set; } = ownerId;
    public PendingScenarioTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a pending scenario template in status proposed.</summary>
    public static PendingScenarioTemplate NewPendingScenarioTemplate(string code, string name)
    {
        return new PendingScenarioTemplate(
            code,
            name,
            "",
            0.0,
            "",
            "",
            0,
            0,
            "",
            0.0,
            "",
            PendingScenarioTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingScenarioTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingScenarioTemplateStatus next)
    {
        switch (Status)
        {
            case PendingScenarioTemplateStatus.Proposed:
                return next == PendingScenarioTemplateStatus.Rejected || next == PendingScenarioTemplateStatus.Withdrawn;
            case PendingScenarioTemplateStatus.Approved:
                switch (next)
                {
                    case PendingScenarioTemplateStatus.Rejected:
                    case PendingScenarioTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
            case PendingScenarioTemplateStatus.Rejected:
                switch (next)
                {
                    case PendingScenarioTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingScenarioTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending scenario template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pending scenario template"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)EndsAt - FloorValue, 2);
    }

    public PendingScenarioTemplate Copy()
    {
        return new PendingScenarioTemplate(
            Code,
            Name,
            Unit,
            Ceiling,
            SalaryBand,
            Checksum,
            DurationMinutes,
            EndsAt,
            Currency,
            FloorValue,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
