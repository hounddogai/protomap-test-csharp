namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey request is in its life cycle.</summary>
public enum PulseSurveyRequestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pulse survey request and its current status.</summary>
public sealed class PulseSurveyRequest(string code, string title, int attempts, string currency, int quantity, bool visible, int revision, string costCenter, double discount, double budget, string department, PulseSurveyRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public string Currency { get; set; } = currency;
    public int Quantity { get; set; } = quantity;
    public bool Visible { get; set; } = visible;
    public int Revision { get; set; } = revision;
    public string CostCenter { get; set; } = costCenter;
    public double Discount { get; set; } = discount;
    public double Budget { get; set; } = budget;
    public string Department { get; set; } = department;
    public PulseSurveyRequestStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey request in status idle.</summary>
    public static PulseSurveyRequest NewPulseSurveyRequest(string code, string title)
    {
        return new PulseSurveyRequest(
            code,
            title,
            0,
            "",
            0,
            false,
            0,
            "",
            0.0,
            0.0,
            "",
            PulseSurveyRequestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyRequestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyRequestStatus next)
    {
        switch (Status)
        {
            case PulseSurveyRequestStatus.Idle:
                switch (next)
                {
                    case PulseSurveyRequestStatus.Ready:
                        return true;
                }
                return false;
            case PulseSurveyRequestStatus.Warming:
                switch (next)
                {
                    case PulseSurveyRequestStatus.Ready:
                        return true;
                }
                return false;
            case PulseSurveyRequestStatus.Ready:
                return next == PulseSurveyRequestStatus.Draining || next == PulseSurveyRequestStatus.Stopped;
            case PulseSurveyRequestStatus.Draining:
                switch (next)
                {
                    case PulseSurveyRequestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Discount + (double)Revision, 2);
    }

    public PulseSurveyRequest Copy()
    {
        return new PulseSurveyRequest(
            Code,
            Title,
            Attempts,
            Currency,
            Quantity,
            Visible,
            Revision,
            CostCenter,
            Discount,
            Budget,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
