namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary training due batch is in its life cycle.</summary>
public enum PrimaryTrainingDueBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One primary training due batch, with what the planners need to act on it.</summary>
public sealed class PrimaryTrainingDueBatch(string code, string name, string salaryBand, double rating, string ownerId, string team, string location, PrimaryTrainingDueBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public double Rating { get; set; } = rating;
    public string OwnerId { get; set; } = ownerId;
    public string Team { get; set; } = team;
    public string Location { get; set; } = location;
    public PrimaryTrainingDueBatchStatus Status { get; set; } = status;

    /// <summary>Starts a primary training due batch in status created.</summary>
    public static PrimaryTrainingDueBatch NewPrimaryTrainingDueBatch(string code, string name)
    {
        return new PrimaryTrainingDueBatch(code, name, "", 0.0, "", "", "", PrimaryTrainingDueBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryTrainingDueBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryTrainingDueBatchStatus next)
    {
        switch (Status)
        {
            case PrimaryTrainingDueBatchStatus.Created:
                switch (next)
                {
                    case PrimaryTrainingDueBatchStatus.Published:
                        return true;
                }
                return false;
            case PrimaryTrainingDueBatchStatus.Validated:
                switch (next)
                {
                    case PrimaryTrainingDueBatchStatus.Published:
                    case PrimaryTrainingDueBatchStatus.Retired:
                        return true;
                }
                return false;
            case PrimaryTrainingDueBatchStatus.Published:
                switch (next)
                {
                    case PrimaryTrainingDueBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryTrainingDueBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary training due batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary training due batch"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public PrimaryTrainingDueBatch Copy()
    {
        return new PrimaryTrainingDueBatch(Code, Name, SalaryBand, Rating, OwnerId, Team, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
