namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run review is in its life cycle.</summary>
public enum PayRunReviewStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One pay run review, with what the planners need to act on it.</summary>
public sealed class PayRunReview(string code, string title, int batchSize, int version, bool archived, string phone, string ownerId, double hours, PayRunReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public int Version { get; set; } = version;
    public bool Archived { get; set; } = archived;
    public string Phone { get; set; } = phone;
    public string OwnerId { get; set; } = ownerId;
    public double Hours { get; set; } = hours;
    public PayRunReviewStatus Status { get; set; } = status;

    /// <summary>Starts a pay run review in status requested.</summary>
    public static PayRunReview NewPayRunReview(string code, string title)
    {
        return new PayRunReview(code, title, 0, 0, false, "", "", 0.0, PayRunReviewStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunReviewStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunReviewStatus next)
    {
        switch (Status)
        {
            case PayRunReviewStatus.Requested:
                return next == PayRunReviewStatus.Revoked || next == PayRunReviewStatus.Expired;
            case PayRunReviewStatus.Granted:
                switch (next)
                {
                    case PayRunReviewStatus.Revoked:
                        return true;
                }
                return false;
            case PayRunReviewStatus.Revoked:
                switch (next)
                {
                    case PayRunReviewStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayRunReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Hours * (double)Version, 2);
    }

    public PayRunReview Copy()
    {
        return new PayRunReview(Code, Title, BatchSize, Version, Archived, Phone, OwnerId, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
