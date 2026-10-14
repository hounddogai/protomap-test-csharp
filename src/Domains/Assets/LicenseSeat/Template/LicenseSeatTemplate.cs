namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat template is in its life cycle.</summary>
public enum LicenseSeatTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a license seat template and its current status.</summary>
public sealed class LicenseSeatTemplate(string code, string name, string grade, int updatedAt, string managerId, string summary, double minimum, int leaveDays, bool visible, string locale, int batchSize, LicenseSeatTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ManagerId { get; set; } = managerId;
    public string Summary { get; set; } = summary;
    public double Minimum { get; set; } = minimum;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Visible { get; set; } = visible;
    public string Locale { get; set; } = locale;
    public int BatchSize { get; set; } = batchSize;
    public LicenseSeatTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a license seat template in status idle.</summary>
    public static LicenseSeatTemplate NewLicenseSeatTemplate(string code, string name)
    {
        return new LicenseSeatTemplate(
            code,
            name,
            "",
            0,
            "",
            "",
            0.0,
            0,
            false,
            "",
            0,
            LicenseSeatTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatTemplateStatus next)
    {
        switch (Status)
        {
            case LicenseSeatTemplateStatus.Idle:
                switch (next)
                {
                    case LicenseSeatTemplateStatus.Warming:
                    case LicenseSeatTemplateStatus.Stopped:
                        return true;
                }
                return false;
            case LicenseSeatTemplateStatus.Warming:
                return next == LicenseSeatTemplateStatus.Draining;
            case LicenseSeatTemplateStatus.Ready:
                switch (next)
                {
                    case LicenseSeatTemplateStatus.Draining:
                    case LicenseSeatTemplateStatus.Stopped:
                        return true;
                }
                return false;
            case LicenseSeatTemplateStatus.Draining:
                switch (next)
                {
                    case LicenseSeatTemplateStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)LeaveDays - Minimum, 2);
    }

    public LicenseSeatTemplate Copy()
    {
        return new LicenseSeatTemplate(
            Code,
            Name,
            Grade,
            UpdatedAt,
            ManagerId,
            Summary,
            Minimum,
            LeaveDays,
            Visible,
            Locale,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
