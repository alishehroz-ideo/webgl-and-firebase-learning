using BookLab.Models;

namespace BookLab.Core.Events
{
    // Navigation events — raised by screens, handled by AppRoot to swap screens.
    public struct OpenBookRequest { public BookModel Book; }
    public struct CreateBookRequest { }
    public struct GoHomeRequest { }
}
