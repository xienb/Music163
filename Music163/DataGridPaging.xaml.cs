using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Music163
{
    public class GridPagingEventArgs : RoutedEventArgs
    {
        public GridPagingEventArgs(int size, int index)
        {
            PageSize = size;
            PageIndex = index;
        }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
    }

    public delegate void GridPagingEventHandler(object sender, GridPagingEventArgs e);

    /// <summary>
    /// DataGridPaging.xaml 的交互逻辑
    /// </summary>
    public partial class DataGridPaging : UserControl
    {
        public DataGridPaging()
        {
            InitializeComponent();
        }

        public static readonly RoutedEvent GridPagingEvent = EventManager.RegisterRoutedEvent("GridPaging", RoutingStrategy.Bubble, typeof(GridPagingEventHandler), typeof(DataGridPaging));
        public event GridPagingEventHandler GridPaging
        {
            add { this.AddHandler(GridPagingEvent, value); }
            remove { this.RemoveHandler(GridPagingEvent, value); }
        }

        /// <summary>
        /// 创建"..."label
        /// </summary>
        /// <returns></returns>
        private Label CreateDotLabel()
        {
            Label label = new Label();
            label.Width = 36;
            label.Height = 36;
            label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2aa1c8"));
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            label.VerticalContentAlignment = VerticalAlignment.Center;
            label.Content = "...";
            return label;
        }


        private int _currentIndex = 1;
        /// <summary>
        /// 记录当前所选页码
        /// </summary>
        public int CurrentIndex
        {
            get { return _currentIndex; }
        }

        private int _currentSize = 20;
        /// <summary>
        /// 当前一页长度
        /// </summary>
        public int CurrentSize
        {
            get { return _currentSize; }
        }

        private int _currentCount = 0;
        /// <summary>
        /// 当前总条数
        /// </summary>
        public int CurrentCount
        {
            get { return _currentCount; }
        }

        /// <summary>
        /// 当前总页数
        /// </summary>
        private int _currentPageCount = 0;

        /// <summary>
        /// 非选中的数字
        /// </summary>
        /// <param name="number"></param>
        /// <param name="borderThickness"></param>
        /// <returns></returns>
        private Border CreateUnSelectNumberBorder(int number, Thickness borderThickness)
        {
            Border border = new Border();
            border.MinWidth = 36;
            border.Cursor = Cursors.Hand;
            border.Height = 36;
            border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ddd"));
            border.BorderThickness = borderThickness;
            border.CornerRadius = new CornerRadius(2);
            border.Margin = new Thickness(8, 0, 0, 0);
            border.Child = new Label()
            {
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666")),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Content = number.ToString()
            };
            return border;
        }

        /// <summary>
        /// 当前选中的数字
        /// </summary>
        /// <param name="number"></param>
        /// <param name="borderThickness"></param>
        /// <returns></returns>
        private Border CreateSelectNumberBorder(int number, Thickness borderThickness)
        {
            Border border = new Border();
            border.MinWidth = 36;
            border.Height = 36;
            border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ddd"));
            border.BorderThickness = borderThickness;
            border.CornerRadius = new CornerRadius(2);
            border.Margin = new Thickness(8, 0, 0, 0);
            border.Child = new Label()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2aa1c8")),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Content = number.ToString()
            };
            return border;
        }
        /// <summary>
        /// 列表查询以后调用该方法重置分页控件
        /// </summary>
        /// <param name="size">每页条数</param>
        /// <param name="index">当前第几页</param>
        /// <param name="sumcount">总共条数</param>
        public void ResetPage(int size, int index, int sumcount)
        {
            _currentIndex = index;
            _currentSize = size;
            _currentCount = sumcount;
            SP_NumberContainer.Children.Clear();
            _currentPageCount = sumcount / size;
            if (sumcount % size > 0)
            {
                _currentPageCount++;
            }
            if (_currentPageCount <= 8)//直接显示所有页号
            {
                for (int i = 1; i <= _currentPageCount; i++)
                {
                    if (i != index)//非选中
                    {
                        Border unSelectNumber = CreateUnSelectNumberBorder(i, new Thickness(1, 1, 1, 1));
                        unSelectNumber.MouseLeftButtonDown += UnSelectNumber_MouseLeftButtonDown;
                        SP_NumberContainer.Children.Add(unSelectNumber);
                    }
                    else//选中
                    {
                        Border b = CreateSelectNumberBorder(i, new Thickness(1, 1, 1, 1));
                        SP_NumberContainer.Children.Add(b);
                    }
                }
            }
            else//两边增加...
            {
                if (index <= 4)//当前选中的是头四页
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        if (i == index)
                        {
                            Border b = CreateSelectNumberBorder(i, new Thickness(1, 1, 1, 1));
                            SP_NumberContainer.Children.Add(b);
                        }
                        else
                        {
                            Border unSelectNumber = CreateUnSelectNumberBorder(i, new Thickness(1, 1, 1, 1));
                            unSelectNumber.MouseLeftButtonDown += UnSelectNumber_MouseLeftButtonDown;
                            SP_NumberContainer.Children.Add(unSelectNumber);
                        }
                    }
                    SP_NumberContainer.Children.Add(CreateDotLabel());

                    Border lastNumber = CreateUnSelectNumberBorder(_currentPageCount, new Thickness(1, 1, 1, 1));
                    lastNumber.MouseLeftButtonDown += UnSelectNumber_MouseLeftButtonDown;
                    SP_NumberContainer.Children.Add(lastNumber);
                }
                else if (index > 4 && index <= _currentPageCount - 4)//中间
                {
                    Border firstNumber = CreateUnSelectNumberBorder(1, new Thickness(1, 1, 1, 1));
                    firstNumber.MouseLeftButtonDown += UnSelectNumber_MouseLeftButtonDown;
                    SP_NumberContainer.Children.Add(firstNumber);
                    SP_NumberContainer.Children.Add(CreateDotLabel());

                    for (int i = index - 2; i <= index + 2; i++)
                    {
                        if (i == index - 2)
                        {
                            Border b = CreateUnSelectNumberBorder(i, new Thickness(1, 1, 1, 1));
                            b.MouseLeftButtonDown += UnSelectNumber_MouseLeftButtonDown;
                            SP_NumberContainer.Children.Add(b);
                            continue;
                        }
                        else
                        {
                            if (i == index)
                            {
                                Border b = CreateSelectNumberBorder(i, new Thickness(1, 1, 1, 1));
                                SP_NumberContainer.Children.Add(b);
                            }
                            else
                            {
                                Border b = CreateUnSelectNumberBorder(i, new Thickness(1, 1, 1, 1));
                                b.MouseLeftButtonDown += UnSelectNumber_MouseLeftButtonDown;
                                SP_NumberContainer.Children.Add(b);
                            }
                        }
                    }

                    SP_NumberContainer.Children.Add(CreateDotLabel());
                    Border lastNumber = CreateUnSelectNumberBorder(_currentPageCount, new Thickness(1, 1, 1, 1));
                    lastNumber.MouseLeftButtonDown += UnSelectNumber_MouseLeftButtonDown;
                    SP_NumberContainer.Children.Add(lastNumber);
                }
                else if (index > _currentPageCount - 4)//尾四页
                {
                    Border firstNumber = CreateUnSelectNumberBorder(1, new Thickness(1, 1, 1, 1));
                    firstNumber.MouseLeftButtonDown += UnSelectNumber_MouseLeftButtonDown;
                    SP_NumberContainer.Children.Add(firstNumber);
                    SP_NumberContainer.Children.Add(CreateDotLabel());
                    for (int i = _currentPageCount - 4; i <= _currentPageCount; i++)
                    {
                        if (i == _currentPageCount - 4)
                        {
                            Border b = CreateUnSelectNumberBorder(i, new Thickness(1, 1, 1, 1));
                            b.MouseLeftButtonDown += UnSelectNumber_MouseLeftButtonDown;
                            SP_NumberContainer.Children.Add(b);
                            continue;
                        }
                        if (i == index)
                        {
                            Border b = CreateSelectNumberBorder(i, new Thickness(1, 1, 1, 1));
                            SP_NumberContainer.Children.Add(b);
                        }
                        else
                        {
                            Border b = CreateUnSelectNumberBorder(i, new Thickness(1, 1, 1, 1));
                            b.MouseLeftButtonDown += UnSelectNumber_MouseLeftButtonDown;
                            SP_NumberContainer.Children.Add(b);
                        }
                    }
                }
            }
            Label_SumCount.Content = sumcount.ToString();
            TB_CurrentIndex.Text = index.ToString();
        }

        /// <summary>
        /// 换页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnSelectNumber_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border b = sender as Border;
            GridPagingEventArgs args = new GridPagingEventArgs(20, Convert.ToInt32((b.Child as Label).Content));
            args.RoutedEvent = GridPagingEvent;
            RaiseEvent(args);
        }

        /// <summary>
        /// 前一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_Previous_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentIndex == 1) return;
            GridPagingEventArgs args = new GridPagingEventArgs(20, _currentIndex - 1);
            args.RoutedEvent = GridPagingEvent;
            RaiseEvent(args);
        }

        /// <summary>
        /// 后一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_Next_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentIndex == _currentPageCount) return;
            GridPagingEventArgs args = new GridPagingEventArgs(20, _currentIndex + 1);
            args.RoutedEvent = GridPagingEvent;
            RaiseEvent(args);
        }

        /// <summary>
        /// 更换每页条数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CB_PageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentIndex = 1;
            GridPagingEventArgs args = new GridPagingEventArgs(20, _currentIndex);
            args.RoutedEvent = GridPagingEvent;
            RaiseEvent(args);
        }

        /// <summary>
        /// 检测粘贴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TB_CurrentIndex_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!isNumberic(text))
                { e.CancelCommand(); }
            }
            else { e.CancelCommand(); }
        }

        /// <summary>
        /// 是否数字
        /// </summary>
        /// <param name="_string"></param>
        /// <returns></returns>
        public static bool isNumberic(string _string)
        {
            if (string.IsNullOrEmpty(_string))
                return false;
            foreach (char c in _string)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        private void TB_CurrentIndex_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;

        }

        private void TB_CurrentIndex_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!isNumberic(e.Text))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;

            }
        }

        private void TB_CurrentIndex_TextChanged(object sender, TextChangedEventArgs e)
        {
            TB_CurrentIndex.Width = 36.0 + TB_CurrentIndex.Text.Length * 8;
            //int index;
            //if (int.TryParse(TB_CurrentIndex.Text, out index))
            //{
            //    if (index != _currentIndex)
            //    {
            //        if (index <= _currentPageCount)
            //        {
            //            _currentIndex = index;
            //        }
            //        else
            //        {
            //            _currentIndex = _currentPageCount;
            //        }
            //        GridPagingEventArgs args = new GridPagingEventArgs(Convert.ToInt32((CB_PageSize.SelectedItem as ComboBoxItem).Content), _currentIndex);
            //        args.RoutedEvent = GridPagingEvent;
            //        RaiseEvent(args);
            //    }
            //}
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NavigateTo(object sender, RoutedEventArgs e)
        {
            int index;
            if (int.TryParse(TB_CurrentIndex.Text, out index))
            {
                if (index != _currentIndex)
                {
                    if (index <= _currentPageCount)
                    {
                        _currentIndex = index;
                    }
                    else
                    {
                        _currentIndex = _currentPageCount;
                    }
                    GridPagingEventArgs args = new GridPagingEventArgs(20, _currentIndex);
                    args.RoutedEvent = GridPagingEvent;
                    RaiseEvent(args);
                }
            }
        }
    }
}
