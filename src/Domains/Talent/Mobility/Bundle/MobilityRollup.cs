namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility rollup is in its life cycle.</summary>
public enum MobilityRollupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A mobility rollup. Its code is unique within a tenant.</summary>
public sealed class MobilityRollup(string code, string title, string skill, int quantity, double maximum, double rating, string source, double minimum, int tenureMonths, string badge, double ceiling, MobilityRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public int Quantity { get; set; } = quantity;
    public double Maximum { get; set; } = maximum;
    public double Rating { get; set; } = rating;
    public string Source { get; set; } = source;
    public double Minimum { get; set; } = minimum;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Badge { get; set; } = badge;
    public double Ceiling { get; set; } = ceiling;
    public MobilityRollupStatus Status { get; set; } = status;

    /// <summary>Starts a mobility rollup in status draft.</summary>
    public static MobilityRollup NewMobilityRollup(string code, string title)
    {
        return new MobilityRollup(code, title, "", 0, 0.0, 0.0, "", 0.0, 0, "", 0.0, MobilityRollupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityRollupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityRollupStatus next)
    {
        switch (Status)
        {
            case MobilityRollupStatus.Draft:
                switch (next)
                {
                    case MobilityRollupStatus.Suspended:
                        return true;
                }
                return false;
            case MobilityRollupStatus.Pending:
                switch (next)
                {
                    case MobilityRollupStatus.Suspended:
                    case MobilityRollupStatus.Closed:
                        return true;
                }
                return false;
            case MobilityRollupStatus.Active:
                return next == MobilityRollupStatus.Suspended || next == MobilityRollupStatus.Closed;
            case MobilityRollupStatus.Suspended:
                switch (next)
                {
                    case MobilityRollupStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Quantity + Ceiling, 2);
    }

    public MobilityRollup Copy()
    {
        return new MobilityRollup(
            Code,
            Title,
            Skill,
            Quantity,
            Maximum,
            Rating,
            Source,
            Minimum,
            TenureMonths,
            Badge,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
