namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey rollup is in its life cycle.</summary>
public enum PulseSurveyRollupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pulse survey rollup. Its code is unique within a tenant.</summary>
public sealed class PulseSurveyRollup(string code, string title, string ownerId, double amount, Dictionary<string, string> attributes, double discount, string notes, int graceDays, string label, PulseSurveyRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public double Amount { get; set; } = amount;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Discount { get; set; } = discount;
    public string Notes { get; set; } = notes;
    public int GraceDays { get; set; } = graceDays;
    public string Label { get; set; } = label;
    public PulseSurveyRollupStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey rollup in status requested.</summary>
    public static PulseSurveyRollup NewPulseSurveyRollup(string code, string title)
    {
        return new PulseSurveyRollup(
            code,
            title,
            "",
            0.0,
            new Dictionary<string, string>(),
            0.0,
            "",
            0,
            "",
            PulseSurveyRollupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyRollupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyRollupStatus next)
    {
        switch (Status)
        {
            case PulseSurveyRollupStatus.Requested:
                return next == PulseSurveyRollupStatus.Granted || next == PulseSurveyRollupStatus.Expired;
            case PulseSurveyRollupStatus.Granted:
                switch (next)
                {
                    case PulseSurveyRollupStatus.Revoked:
                    case PulseSurveyRollupStatus.Expired:
                        return true;
                }
                return false;
            case PulseSurveyRollupStatus.Revoked:
                return next == PulseSurveyRollupStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PulseSurveyRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pulse survey rollup"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)GraceDays * Amount, 2);
    }

    public PulseSurveyRollup Copy()
    {
        return new PulseSurveyRollup(
            Code,
            Title,
            OwnerId,
            Amount,
            Attributes,
            Discount,
            Notes,
            GraceDays,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
