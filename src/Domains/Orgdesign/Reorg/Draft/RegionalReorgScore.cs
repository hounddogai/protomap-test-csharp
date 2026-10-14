namespace Acme.Workforce.Orgdesign.Reorg.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional reorg score is in its life cycle.</summary>
public enum RegionalReorgScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a regional reorg score and its current status.</summary>
public sealed class RegionalReorgScore(string code, string name, string salaryBand, List<string> tags, int updatedAt, string country, string phone, string unit, RegionalReorgScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public List<string> Tags { get; set; } = tags;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Country { get; set; } = country;
    public string Phone { get; set; } = phone;
    public string Unit { get; set; } = unit;
    public RegionalReorgScoreStatus Status { get; set; } = status;

    /// <summary>Starts a regional reorg score in status open.</summary>
    public static RegionalReorgScore NewRegionalReorgScore(string code, string name)
    {
        return new RegionalReorgScore(
            code,
            name,
            "",
            new List<string>(),
            0,
            "",
            "",
            "",
            RegionalReorgScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalReorgScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalReorgScoreStatus next)
    {
        switch (Status)
        {
            case RegionalReorgScoreStatus.Open:
                switch (next)
                {
                    case RegionalReorgScoreStatus.Reopened:
                    case RegionalReorgScoreStatus.Archived:
                        return true;
                }
                return false;
            case RegionalReorgScoreStatus.InReview:
                switch (next)
                {
                    case RegionalReorgScoreStatus.Resolved:
                        return true;
                }
                return false;
            case RegionalReorgScoreStatus.Resolved:
                switch (next)
                {
                    case RegionalReorgScoreStatus.Archived:
                        return true;
                }
                return false;
            case RegionalReorgScoreStatus.Reopened:
                switch (next)
                {
                    case RegionalReorgScoreStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalReorgScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional reorg score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public RegionalReorgScore Copy()
    {
        return new RegionalReorgScore(Code, Name, SalaryBand, Tags, UpdatedAt, Country, Phone, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
