namespace Acme.Workforce.Workplace.Tests.Booking.Review;

using Acme.Workforce.Workplace.Booking.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BookingReviewStoreChecks
{
    public static void MemoryBookingReviewStoreFindsWhatItSaved()
    {
        var store = BookingReviewStore.NewMemoryBookingReviewStore();
        var item = BookingReview.NewBookingReview("code-75", "title-281");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryBookingReviewStoreFindsWhatItSaved();
    }
}
