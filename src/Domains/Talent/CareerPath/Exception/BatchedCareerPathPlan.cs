namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched career path plan is in its life cycle.</summary>
public enum BatchedCareerPathPlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One batched career path plan, with what the planners need to act on it.</summary>
public sealed class BatchedCareerPathPlan(string code, string name, double cost, double discount, string location, int endsAt, BatchedCareerPathPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public double Discount { get; set; } = discount;
    public string Location { get; set; } = location;
    public int EndsAt { get; set; } = endsAt;
    public BatchedCareerPathPlanStatus Status { get; set; } = status;

    /// <summary>Starts a batched career path plan in status created.</summary>
    public static BatchedCareerPathPlan NewBatchedCareerPathPlan(string code, string name)
    {
        return new BatchedCareerPathPlan(code, name, 0.0, 0.0, "", 0, BatchedCareerPathPlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedCareerPathPlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedCareerPathPlanStatus next)
    {
        switch (Status)
        {
            case BatchedCareerPathPlanStatus.Created:
                return next == BatchedCareerPathPlanStatus.Validated;
            case BatchedCareerPathPlanStatus.Validated:
                return next == BatchedCareerPathPlanStatus.Published;
            case BatchedCareerPathPlanStatus.Published:
                return next == BatchedCareerPathPlanStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BatchedCareerPathPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched career path plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Cost + (double)EndsAt, 2);
    }

    public BatchedCareerPathPlan Copy()
    {
        return new BatchedCareerPathPlan(Code, Name, Cost, Discount, Location, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
