namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted training due profile is in its life cycle.</summary>
public enum WeightedTrainingDueProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A weighted training due profile. Its code is unique within a tenant.</summary>
public sealed class WeightedTrainingDueProfile(string code, string title, string reviewer, double fte, double amount, int sequence, string skill, WeightedTrainingDueProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public double Fte { get; set; } = fte;
    public double Amount { get; set; } = amount;
    public int Sequence { get; set; } = sequence;
    public string Skill { get; set; } = skill;
    public WeightedTrainingDueProfileStatus Status { get; set; } = status;

    /// <summary>Starts a weighted training due profile in status draft.</summary>
    public static WeightedTrainingDueProfile NewWeightedTrainingDueProfile(string code, string title)
    {
        return new WeightedTrainingDueProfile(
            code,
            title,
            "",
            0.0,
            0.0,
            0,
            "",
            WeightedTrainingDueProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedTrainingDueProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedTrainingDueProfileStatus next)
    {
        switch (Status)
        {
            case WeightedTrainingDueProfileStatus.Draft:
                switch (next)
                {
                    case WeightedTrainingDueProfileStatus.Suspended:
                        return true;
                }
                return false;
            case WeightedTrainingDueProfileStatus.Pending:
                switch (next)
                {
                    case WeightedTrainingDueProfileStatus.Active:
                        return true;
                }
                return false;
            case WeightedTrainingDueProfileStatus.Active:
                switch (next)
                {
                    case WeightedTrainingDueProfileStatus.Suspended:
                    case WeightedTrainingDueProfileStatus.Closed:
                        return true;
                }
                return false;
            case WeightedTrainingDueProfileStatus.Suspended:
                return next == WeightedTrainingDueProfileStatus.Closed;
        }
        return false;
    }

    public void MoveTo(WeightedTrainingDueProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted training due profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted training due profile"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Sequence + Fte, 2);
    }

    public WeightedTrainingDueProfile Copy()
    {
        return new WeightedTrainingDueProfile(Code, Title, Reviewer, Fte, Amount, Sequence, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
