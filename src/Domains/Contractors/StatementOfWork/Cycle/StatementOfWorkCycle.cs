namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work cycle is in its life cycle.</summary>
public enum StatementOfWorkCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One statement of work cycle, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkCycle(string code, string label, string salaryBand, string title, string checksum, List<string> tags, double minimum, double fte, string phone, double discount, string skill, StatementOfWorkCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public List<string> Tags { get; set; } = tags;
    public double Minimum { get; set; } = minimum;
    public double Fte { get; set; } = fte;
    public string Phone { get; set; } = phone;
    public double Discount { get; set; } = discount;
    public string Skill { get; set; } = skill;
    public StatementOfWorkCycleStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work cycle in status requested.</summary>
    public static StatementOfWorkCycle NewStatementOfWorkCycle(string code, string label)
    {
        return new StatementOfWorkCycle(
            code,
            label,
            "",
            "",
            "",
            new List<string>(),
            0.0,
            0.0,
            "",
            0.0,
            "",
            StatementOfWorkCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkCycleStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkCycleStatus.Requested:
                return next == StatementOfWorkCycleStatus.Granted || next == StatementOfWorkCycleStatus.Expired;
            case StatementOfWorkCycleStatus.Granted:
                return next == StatementOfWorkCycleStatus.Revoked;
            case StatementOfWorkCycleStatus.Revoked:
                return next == StatementOfWorkCycleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double NetExposure()
    {
        return Math.Round(Minimum - Discount, 2);
    }

    public StatementOfWorkCycle Copy()
    {
        return new StatementOfWorkCycle(
            Code,
            Label,
            SalaryBand,
            Title,
            Checksum,
            Tags,
            Minimum,
            Fte,
            Phone,
            Discount,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
