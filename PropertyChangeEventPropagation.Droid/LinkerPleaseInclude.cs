using System.Collections.Specialized;
using Android.Widget;

namespace PropertyChangeEventPropagation.Droid
{
	// This class is never actually executed, but when Xamarin linking is enabled it does how to ensure types and properties
	// are preserved in the deployed app
    public class LinkerPleaseInclude
    {
        public void Include(Button button)
        {
            button.Click += (s,e) => button.Text = button.Text + "";
        }

        public void Include(CheckBox checkBox)
        {
            checkBox.CheckedChange += (sender, args) => checkBox.Checked = !checkBox.Checked;
        }

        public void Include(TextView text)
        {
            text.TextChanged += (sender, args) => text.Text = "" + text.Text;
        }

        public void Include(CompoundButton cb)
        {
            cb.CheckedChange += (sender, args) => cb.Checked = !cb.Checked;
        }

        public void Include(SeekBar sb)
        {
            sb.ProgressChanged += (sender, args) => sb.Progress = sb.Progress + 1;
        }

        public void Include(INotifyCollectionChanged changed)
        {
            changed.CollectionChanged += (s,e) => { var test = string.Format("{0}{1}{2}{3}{4}", e.Action,e.NewItems, e.NewStartingIndex, e.OldItems, e.OldStartingIndex); } ;
        }
    }
}
