namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient warranty forecast is in its life cycle.</summary>
public enum LenientWarrantyForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A lenient warranty forecast. Its code is unique within a tenant.</summary>
public sealed class LenientWarrantyForecast(string code, string label, Dictionary<string, string> attributes, int attempts, int durationMinutes, string contactEmail, string title, LenientWarrantyForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Attempts { get; set; } = attempts;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ContactEmail { get; set; } = contactEmail;
    public string Title { get; set; } = title;
    public LenientWarrantyForecastStatus Status { get; set; } = status;

    /// <summary>Starts a lenient warranty forecast in status proposed.</summary>
    public static LenientWarrantyForecast NewLenientWarrantyForecast(string code, string label)
    {
        return new LenientWarrantyForecast(
            code,
            label,
            new Dictionary<string, string>(),
            0,
            0,
            "",
            "",
            LenientWarrantyForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientWarrantyForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientWarrantyForecastStatus next)
    {
        switch (Status)
        {
            case LenientWarrantyForecastStatus.Proposed:
                switch (next)
                {
                    case LenientWarrantyForecastStatus.Approved:
                    case LenientWarrantyForecastStatus.Rejected:
                        return true;
                }
                return false;
            case LenientWarrantyForecastStatus.Approved:
                switch (next)
                {
                    case LenientWarrantyForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case LenientWarrantyForecastStatus.Rejected:
                return next == LenientWarrantyForecastStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LenientWarrantyForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient warranty forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double WeightedVolume()
    {
        return Math.Round((double)DurationMinutes + (double)Attempts, 2);
    }

    public LenientWarrantyForecast Copy()
    {
        return new LenientWarrantyForecast(
            Code,
            Label,
            Attributes,
            Attempts,
            DurationMinutes,
            ContactEmail,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
