namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan window is in its life cycle.</summary>
public enum SuccessionPlanWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A succession plan window. Its code is unique within a tenant.</summary>
public sealed class SuccessionPlanWindow(string code, string label, string locale, string summary, int updatedAt, string externalRef, int quantity, SuccessionPlanWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string Summary { get; set; } = summary;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ExternalRef { get; set; } = externalRef;
    public int Quantity { get; set; } = quantity;
    public SuccessionPlanWindowStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan window in status requested.</summary>
    public static SuccessionPlanWindow NewSuccessionPlanWindow(string code, string label)
    {
        return new SuccessionPlanWindow(code, label, "", "", 0, "", 0, SuccessionPlanWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanWindowStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanWindowStatus.Requested:
                return next == SuccessionPlanWindowStatus.Granted || next == SuccessionPlanWindowStatus.Revoked;
            case SuccessionPlanWindowStatus.Granted:
                switch (next)
                {
                    case SuccessionPlanWindowStatus.Expired:
                        return true;
                }
                return false;
            case SuccessionPlanWindowStatus.Revoked:
                return next == SuccessionPlanWindowStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Quantity * (double)UpdatedAt, 2);
    }

    public SuccessionPlanWindow Copy()
    {
        return new SuccessionPlanWindow(Code, Label, Locale, Summary, UpdatedAt, ExternalRef, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
