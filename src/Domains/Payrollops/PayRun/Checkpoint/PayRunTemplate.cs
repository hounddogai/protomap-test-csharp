namespace Acme.Workforce.Payrollops.PayRun.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run template is in its life cycle.</summary>
public enum PayRunTemplateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pay run template. Its code is unique within a tenant.</summary>
public sealed class PayRunTemplate(string code, string label, double ratio, int createdAt, string summary, string contactEmail, double minimum, int revision, string grade, double ceiling, string reviewer, PayRunTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public int CreatedAt { get; set; } = createdAt;
    public string Summary { get; set; } = summary;
    public string ContactEmail { get; set; } = contactEmail;
    public double Minimum { get; set; } = minimum;
    public int Revision { get; set; } = revision;
    public string Grade { get; set; } = grade;
    public double Ceiling { get; set; } = ceiling;
    public string Reviewer { get; set; } = reviewer;
    public PayRunTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a pay run template in status planned.</summary>
    public static PayRunTemplate NewPayRunTemplate(string code, string label)
    {
        return new PayRunTemplate(code, label, 0.0, 0, "", "", 0.0, 0, "", 0.0, "", PayRunTemplateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunTemplateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunTemplateStatus next)
    {
        switch (Status)
        {
            case PayRunTemplateStatus.Planned:
                switch (next)
                {
                    case PayRunTemplateStatus.InProgress:
                        return true;
                }
                return false;
            case PayRunTemplateStatus.Scheduled:
                switch (next)
                {
                    case PayRunTemplateStatus.InProgress:
                    case PayRunTemplateStatus.Completed:
                        return true;
                }
                return false;
            case PayRunTemplateStatus.InProgress:
                switch (next)
                {
                    case PayRunTemplateStatus.Completed:
                    case PayRunTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case PayRunTemplateStatus.Completed:
                switch (next)
                {
                    case PayRunTemplateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"pay run template"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Revision * Ceiling, 2);
    }

    public PayRunTemplate Copy()
    {
        return new PayRunTemplate(
            Code,
            Label,
            Ratio,
            CreatedAt,
            Summary,
            ContactEmail,
            Minimum,
            Revision,
            Grade,
            Ceiling,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
