namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive tax form record is in its life cycle.</summary>
public enum AdaptiveTaxFormRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a adaptive tax form record and its current status.</summary>
public sealed class AdaptiveTaxFormRecord(string code, string name, int batchSize, string channel, double amount, string team, double rate, string country, double score, AdaptiveTaxFormRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public string Channel { get; set; } = channel;
    public double Amount { get; set; } = amount;
    public string Team { get; set; } = team;
    public double Rate { get; set; } = rate;
    public string Country { get; set; } = country;
    public double Score { get; set; } = score;
    public AdaptiveTaxFormRecordStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive tax form record in status planned.</summary>
    public static AdaptiveTaxFormRecord NewAdaptiveTaxFormRecord(string code, string name)
    {
        return new AdaptiveTaxFormRecord(
            code,
            name,
            0,
            "",
            0.0,
            "",
            0.0,
            "",
            0.0,
            AdaptiveTaxFormRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTaxFormRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTaxFormRecordStatus next)
    {
        switch (Status)
        {
            case AdaptiveTaxFormRecordStatus.Planned:
                return next == AdaptiveTaxFormRecordStatus.Completed;
            case AdaptiveTaxFormRecordStatus.Scheduled:
                return next == AdaptiveTaxFormRecordStatus.Completed || next == AdaptiveTaxFormRecordStatus.Cancelled;
            case AdaptiveTaxFormRecordStatus.InProgress:
                return next == AdaptiveTaxFormRecordStatus.Completed || next == AdaptiveTaxFormRecordStatus.Cancelled;
            case AdaptiveTaxFormRecordStatus.Completed:
                switch (next)
                {
                    case AdaptiveTaxFormRecordStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveTaxFormRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive tax form record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedValue()
    {
        return Math.Round(Rate + Score, 2);
    }

    public AdaptiveTaxFormRecord Copy()
    {
        return new AdaptiveTaxFormRecord(Code, Name, BatchSize, Channel, Amount, Team, Rate, Country, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
