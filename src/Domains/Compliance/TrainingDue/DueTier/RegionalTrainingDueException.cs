namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional training due exception is in its life cycle.</summary>
public enum RegionalTrainingDueExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a regional training due exception and its current status.</summary>
public sealed class RegionalTrainingDueException(string code, string title, double score, double floorValue, int attempts, double cost, int capacity, string category, RegionalTrainingDueExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public double FloorValue { get; set; } = floorValue;
    public int Attempts { get; set; } = attempts;
    public double Cost { get; set; } = cost;
    public int Capacity { get; set; } = capacity;
    public string Category { get; set; } = category;
    public RegionalTrainingDueExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a regional training due exception in status requested.</summary>
    public static RegionalTrainingDueException NewRegionalTrainingDueException(string code, string title)
    {
        return new RegionalTrainingDueException(
            code,
            title,
            0.0,
            0.0,
            0,
            0.0,
            0,
            "",
            RegionalTrainingDueExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTrainingDueExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTrainingDueExceptionStatus next)
    {
        switch (Status)
        {
            case RegionalTrainingDueExceptionStatus.Requested:
                switch (next)
                {
                    case RegionalTrainingDueExceptionStatus.Granted:
                        return true;
                }
                return false;
            case RegionalTrainingDueExceptionStatus.Granted:
                return next == RegionalTrainingDueExceptionStatus.Revoked || next == RegionalTrainingDueExceptionStatus.Expired;
            case RegionalTrainingDueExceptionStatus.Revoked:
                switch (next)
                {
                    case RegionalTrainingDueExceptionStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalTrainingDueExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional training due exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional training due exception"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Capacity * FloorValue, 2);
    }

    public RegionalTrainingDueException Copy()
    {
        return new RegionalTrainingDueException(
            Code,
            Title,
            Score,
            FloorValue,
            Attempts,
            Cost,
            Capacity,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
