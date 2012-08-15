using adisware.juipp.Events.Arguments;
using adisware.juipp.ViewModels;
using adisware.juipp.Views;

namespace adisware.juipp.Events.Handlers
{
    public delegate void OptionChangedHandler<TD>(ViewBase sender, OptionChangedEventArgs<TD> args)  where TD : IViewModel, new();
}