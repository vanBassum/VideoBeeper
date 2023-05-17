using System.ComponentModel;

namespace Beeper
{
    //public class FFMPEG
    //{ 
    //    
    //    public static void AddBeeps(string input, string output, TimeSpan offset, TimeSpan interval)
    //    {
    //        string audioArgs = "";
    //
    //        
    //
    //
    //        string args = $@"ffmpeg -i {input} -filter_complex ""[0]volume=0:enable='between(t,10,15)'[main];sine=d=5:f=800,adelay=10s,pan=stereo|FL=c0|FR=c0[beep];[main][beep]amix=inputs=2"" {}";
    //    }
    //
    //}

    public static class Ext
    {
        public static void InvokeIfRequired(this ISynchronizeInvoke obj, MethodInvoker action)
        {
            if (obj.InvokeRequired)
            {
                var args = new object[0];
                obj.Invoke(action, args);
            }
            else
            {
                action();
            }
        }

        public static T InvokeIfRequired<T>(this ISynchronizeInvoke obj, Func<T> action)
        {
            if (obj.InvokeRequired)
            {
                var args = new object[0];
                return (T)obj.Invoke(action, args);
            }
            else
            {
                return action();
            }
        }

        public static void AddMenuItem(this ToolStrip menu, string menuPath, Action action)
        {
            string[] split = menuPath.Split('/');

            ToolStripMenuItem item = null;

            if (menu.Items[split[0]] is ToolStripMenuItem tsi)
                item = tsi;
            else
            {
                item = new ToolStripMenuItem(split[0]);
                item.Name = split[0];
                menu.Items.Add(item);
            }

            for (int i = 1; i < split.Length; i++)
            {
                string name = split[i];

                if (item.DropDownItems[name] is ToolStripMenuItem tsii)
                    item = tsii;
                else
                {
                    ToolStripMenuItem newItem = new ToolStripMenuItem(name);
                    newItem.Name = name;
                    item.DropDownItems.Add(newItem);
                    item = newItem;
                }

            }

            if (action != null)
                item.Click += (a, b) => action.Invoke();
        }


        public static void AddMenuItem(this ToolStripMenuItem menuItem, string menuPath, Action action)
        {
            string[] split = menuPath.Split('/');

            ToolStripMenuItem item = menuItem;

            for (int i = 1; i < split.Length; i++)
            {
                string name = split[i];

                if (item.DropDownItems[name] is ToolStripMenuItem tsii)
                    item = tsii;
                else
                {
                    ToolStripMenuItem newItem = new ToolStripMenuItem(name);
                    newItem.Name = name;
                    item.DropDownItems.Add(newItem);
                    item = newItem;
                }

            }

            if (action != null)
                item.Click += (a, b) => action.Invoke();
        }




    }
}
