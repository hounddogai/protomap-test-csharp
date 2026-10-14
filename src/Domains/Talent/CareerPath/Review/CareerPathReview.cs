namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path review is in its life cycle.</summary>
public enum CareerPathReviewStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A career path review as the career path team keeps it.</summary>
public sealed class CareerPathReview(string code, string label, string timezone, int graceDays, double weight, string contactEmail, Dictionary<string, string> attributes, bool visible, CareerPathReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public int GraceDays { get; set; } = graceDays;
    public double Weight { get; set; } = weight;
    public string ContactEmail { get; set; } = contactEmail;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Visible { get; set; } = visible;
    public CareerPathReviewStatus Status { get; set; } = status;

    /// <summary>Starts a career path review in status idle.</summary>
    public static CareerPathReview NewCareerPathReview(string code, string label)
    {
        return new CareerPathReview(
            code,
            label,
            "",
            0,
            0.0,
            "",
            new Dictionary<string, string>(),
            false,
            CareerPathReviewStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathReviewStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathReviewStatus next)
    {
        switch (Status)
        {
            case CareerPathReviewStatus.Idle:
                return next == CareerPathReviewStatus.Warming || next == CareerPathReviewStatus.Draining;
            case CareerPathReviewStatus.Warming:
                switch (next)
                {
                    case CareerPathReviewStatus.Ready:
                    case CareerPathReviewStatus.Draining:
                        return true;
                }
                return false;
            case CareerPathReviewStatus.Ready:
                return next == CareerPathReviewStatus.Draining;
            case CareerPathReviewStatus.Draining:
                return next == CareerPathReviewStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CareerPathReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"career path review"} {Code}";
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

    public double ProjectedVolume()
    {
        return Math.Round(Weight - (double)GraceDays, 2);
    }

    public CareerPathReview Copy()
    {
        return new CareerPathReview(
            Code,
            Label,
            Timezone,
            GraceDays,
            Weight,
            ContactEmail,
            Attributes,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
