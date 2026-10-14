namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility record is in its life cycle.</summary>
public enum MobilityRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A mobility record as the mobility team keeps it.</summary>
public sealed class MobilityRecord(string code, string title, int createdAt, string timezone, string jobCode, string salaryBand, string category, double score, MobilityRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Timezone { get; set; } = timezone;
    public string JobCode { get; set; } = jobCode;
    public string SalaryBand { get; set; } = salaryBand;
    public string Category { get; set; } = category;
    public double Score { get; set; } = score;
    public MobilityRecordStatus Status { get; set; } = status;

    /// <summary>Starts a mobility record in status draft.</summary>
    public static MobilityRecord NewMobilityRecord(string code, string title)
    {
        return new MobilityRecord(code, title, 0, "", "", "", "", 0.0, MobilityRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityRecordStatus next)
    {
        switch (Status)
        {
            case MobilityRecordStatus.Draft:
                switch (next)
                {
                    case MobilityRecordStatus.Active:
                    case MobilityRecordStatus.Closed:
                        return true;
                }
                return false;
            case MobilityRecordStatus.Pending:
                switch (next)
                {
                    case MobilityRecordStatus.Suspended:
                    case MobilityRecordStatus.Closed:
                        return true;
                }
                return false;
            case MobilityRecordStatus.Active:
                return next == MobilityRecordStatus.Closed;
            case MobilityRecordStatus.Suspended:
                return next == MobilityRecordStatus.Closed;
        }
        return false;
    }

    public void MoveTo(MobilityRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"mobility record"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Score - (double)CreatedAt, 2);
    }

    public MobilityRecord Copy()
    {
        return new MobilityRecord(Code, Title, CreatedAt, Timezone, JobCode, SalaryBand, Category, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
