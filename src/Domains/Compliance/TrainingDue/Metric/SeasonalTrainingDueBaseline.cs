namespace Acme.Workforce.Compliance.TrainingDue.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal training due baseline is in its life cycle.</summary>
public enum SeasonalTrainingDueBaselineStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a seasonal training due baseline and its current status.</summary>
public sealed class SeasonalTrainingDueBaseline(string code, string name, string badge, string phone, List<string> tags, double rate, double cost, int attempts, SeasonalTrainingDueBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public string Phone { get; set; } = phone;
    public List<string> Tags { get; set; } = tags;
    public double Rate { get; set; } = rate;
    public double Cost { get; set; } = cost;
    public int Attempts { get; set; } = attempts;
    public SeasonalTrainingDueBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal training due baseline in status draft.</summary>
    public static SeasonalTrainingDueBaseline NewSeasonalTrainingDueBaseline(string code, string name)
    {
        return new SeasonalTrainingDueBaseline(
            code,
            name,
            "",
            "",
            new List<string>(),
            0.0,
            0.0,
            0,
            SeasonalTrainingDueBaselineStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalTrainingDueBaselineStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalTrainingDueBaselineStatus next)
    {
        switch (Status)
        {
            case SeasonalTrainingDueBaselineStatus.Draft:
                return next == SeasonalTrainingDueBaselineStatus.Active;
            case SeasonalTrainingDueBaselineStatus.Pending:
                switch (next)
                {
                    case SeasonalTrainingDueBaselineStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalTrainingDueBaselineStatus.Active:
                switch (next)
                {
                    case SeasonalTrainingDueBaselineStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalTrainingDueBaselineStatus.Suspended:
                return next == SeasonalTrainingDueBaselineStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalTrainingDueBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal training due baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
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

    public double WeightedValue()
    {
        return Math.Round(Rate + Cost, 2);
    }

    public SeasonalTrainingDueBaseline Copy()
    {
        return new SeasonalTrainingDueBaseline(Code, Name, Badge, Phone, Tags, Rate, Cost, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
