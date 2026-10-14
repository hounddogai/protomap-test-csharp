namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition plan is in its life cycle.</summary>
public enum RecognitionPlanStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a recognition plan and its current status.</summary>
public sealed class RecognitionPlan(string code, string title, string salaryBand, string region, int quantity, RecognitionPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string Region { get; set; } = region;
    public int Quantity { get; set; } = quantity;
    public RecognitionPlanStatus Status { get; set; } = status;

    /// <summary>Starts a recognition plan in status proposed.</summary>
    public static RecognitionPlan NewRecognitionPlan(string code, string title)
    {
        return new RecognitionPlan(code, title, "", "", 0, RecognitionPlanStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionPlanStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionPlanStatus next)
    {
        switch (Status)
        {
            case RecognitionPlanStatus.Proposed:
                return next == RecognitionPlanStatus.Rejected;
            case RecognitionPlanStatus.Approved:
                return next == RecognitionPlanStatus.Withdrawn;
            case RecognitionPlanStatus.Rejected:
                return next == RecognitionPlanStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RecognitionPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public RecognitionPlan Copy()
    {
        return new RecognitionPlan(Code, Title, SalaryBand, Region, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
