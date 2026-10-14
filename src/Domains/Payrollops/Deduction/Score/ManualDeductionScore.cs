namespace Acme.Workforce.Payrollops.Deduction.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual deduction score is in its life cycle.</summary>
public enum ManualDeductionScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One manual deduction score, with what the planners need to act on it.</summary>
public sealed class ManualDeductionScore(string code, string label, int startsAt, string phone, string summary, int graceDays, ManualDeductionScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public string Phone { get; set; } = phone;
    public string Summary { get; set; } = summary;
    public int GraceDays { get; set; } = graceDays;
    public ManualDeductionScoreStatus Status { get; set; } = status;

    /// <summary>Starts a manual deduction score in status idle.</summary>
    public static ManualDeductionScore NewManualDeductionScore(string code, string label)
    {
        return new ManualDeductionScore(code, label, 0, "", "", 0, ManualDeductionScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualDeductionScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualDeductionScoreStatus next)
    {
        switch (Status)
        {
            case ManualDeductionScoreStatus.Idle:
                switch (next)
                {
                    case ManualDeductionScoreStatus.Draining:
                        return true;
                }
                return false;
            case ManualDeductionScoreStatus.Warming:
                return next == ManualDeductionScoreStatus.Stopped;
            case ManualDeductionScoreStatus.Ready:
                switch (next)
                {
                    case ManualDeductionScoreStatus.Draining:
                    case ManualDeductionScoreStatus.Stopped:
                        return true;
                }
                return false;
            case ManualDeductionScoreStatus.Draining:
                return next == ManualDeductionScoreStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ManualDeductionScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual deduction score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"manual deduction score"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)StartsAt - (double)GraceDays, 2);
    }

    public ManualDeductionScore Copy()
    {
        return new ManualDeductionScore(Code, Label, StartsAt, Phone, Summary, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
