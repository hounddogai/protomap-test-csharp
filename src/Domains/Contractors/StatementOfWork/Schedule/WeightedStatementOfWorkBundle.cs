namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted statement of work bundle is in its life cycle.</summary>
public enum WeightedStatementOfWorkBundleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a weighted statement of work bundle and its current status.</summary>
public sealed class WeightedStatementOfWorkBundle(string code, string name, string grade, string unit, string phone, string currency, int durationMinutes, int limit, WeightedStatementOfWorkBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public string Unit { get; set; } = unit;
    public string Phone { get; set; } = phone;
    public string Currency { get; set; } = currency;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Limit { get; set; } = limit;
    public WeightedStatementOfWorkBundleStatus Status { get; set; } = status;

    /// <summary>Starts a weighted statement of work bundle in status created.</summary>
    public static WeightedStatementOfWorkBundle NewWeightedStatementOfWorkBundle(string code, string name)
    {
        return new WeightedStatementOfWorkBundle(
            code,
            name,
            "",
            "",
            "",
            "",
            0,
            0,
            WeightedStatementOfWorkBundleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedStatementOfWorkBundleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedStatementOfWorkBundleStatus next)
    {
        switch (Status)
        {
            case WeightedStatementOfWorkBundleStatus.Created:
                return next == WeightedStatementOfWorkBundleStatus.Validated || next == WeightedStatementOfWorkBundleStatus.Retired;
            case WeightedStatementOfWorkBundleStatus.Validated:
                switch (next)
                {
                    case WeightedStatementOfWorkBundleStatus.Published:
                        return true;
                }
                return false;
            case WeightedStatementOfWorkBundleStatus.Published:
                return next == WeightedStatementOfWorkBundleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WeightedStatementOfWorkBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted statement of work bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted statement of work bundle"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Limit + (double)DurationMinutes, 2);
    }

    public WeightedStatementOfWorkBundle Copy()
    {
        return new WeightedStatementOfWorkBundle(
            Code,
            Name,
            Grade,
            Unit,
            Phone,
            Currency,
            DurationMinutes,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
