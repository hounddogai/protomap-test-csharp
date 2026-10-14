namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional pay run template is in its life cycle.</summary>
public enum RegionalPayRunTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a regional pay run template and its current status.</summary>
public sealed class RegionalPayRunTemplate(string code, string label, string phone, int endsAt, int tenureMonths, string channel, double discount, double amount, RegionalPayRunTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public int EndsAt { get; set; } = endsAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Channel { get; set; } = channel;
    public double Discount { get; set; } = discount;
    public double Amount { get; set; } = amount;
    public RegionalPayRunTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a regional pay run template in status open.</summary>
    public static RegionalPayRunTemplate NewRegionalPayRunTemplate(string code, string label)
    {
        return new RegionalPayRunTemplate(code, label, "", 0, 0, "", 0.0, 0.0, RegionalPayRunTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPayRunTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPayRunTemplateStatus next)
    {
        switch (Status)
        {
            case RegionalPayRunTemplateStatus.Open:
                switch (next)
                {
                    case RegionalPayRunTemplateStatus.Resolved:
                    case RegionalPayRunTemplateStatus.Archived:
                        return true;
                }
                return false;
            case RegionalPayRunTemplateStatus.InReview:
                switch (next)
                {
                    case RegionalPayRunTemplateStatus.Reopened:
                        return true;
                }
                return false;
            case RegionalPayRunTemplateStatus.Resolved:
                switch (next)
                {
                    case RegionalPayRunTemplateStatus.Reopened:
                    case RegionalPayRunTemplateStatus.Archived:
                        return true;
                }
                return false;
            case RegionalPayRunTemplateStatus.Reopened:
                return next == RegionalPayRunTemplateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RegionalPayRunTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional pay run template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Discount + (double)TenureMonths, 2);
    }

    public RegionalPayRunTemplate Copy()
    {
        return new RegionalPayRunTemplate(
            Code,
            Label,
            Phone,
            EndsAt,
            TenureMonths,
            Channel,
            Discount,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
