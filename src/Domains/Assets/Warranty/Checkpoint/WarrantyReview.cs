namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty review is in its life cycle.</summary>
public enum WarrantyReviewStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A warranty review as the warranty team keeps it.</summary>
public sealed class WarrantyReview(string code, string name, bool active, string grade, string location, double amount, WarrantyReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string Grade { get; set; } = grade;
    public string Location { get; set; } = location;
    public double Amount { get; set; } = amount;
    public WarrantyReviewStatus Status { get; set; } = status;

    /// <summary>Starts a warranty review in status idle.</summary>
    public static WarrantyReview NewWarrantyReview(string code, string name)
    {
        return new WarrantyReview(code, name, false, "", "", 0.0, WarrantyReviewStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyReviewStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyReviewStatus next)
    {
        switch (Status)
        {
            case WarrantyReviewStatus.Idle:
                switch (next)
                {
                    case WarrantyReviewStatus.Warming:
                        return true;
                }
                return false;
            case WarrantyReviewStatus.Warming:
                switch (next)
                {
                    case WarrantyReviewStatus.Stopped:
                        return true;
                }
                return false;
            case WarrantyReviewStatus.Ready:
                switch (next)
                {
                    case WarrantyReviewStatus.Draining:
                    case WarrantyReviewStatus.Stopped:
                        return true;
                }
                return false;
            case WarrantyReviewStatus.Draining:
                return next == WarrantyReviewStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(WarrantyReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public WarrantyReview Copy()
    {
        return new WarrantyReview(Code, Name, Active, Grade, Location, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
