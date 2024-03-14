using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.CodeDom;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Contexts;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //connection string
        CA3_s00242123__DataEntities db = new CA3_s00242123__DataEntities();
        public MainWindow()
        {
            InitializeComponent();
        }
            
        //method to display books and bookings when loading
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var genres = db.BookTbls.Select(b => b.Genre).Distinct().ToArray();            
            cbxGenre.ItemsSource = genres.Append("All");

            ShowBooks();
            ShowBookings();

            //checking date
            DateTime currentDate = DateTime.Now;
            DPStart.DisplayDateStart = currentDate; 

        }
        //method to show books
        private void ShowBooks()
        {
            var query = from b in db.BookTbls
                        orderby b.Book_Id ascending
                        select b;

            dgBooks.ItemsSource = query.ToList();
        }
         //method to show bookings
        private void ShowBookings()
        {
            var query = from b in db.BookingTbls
                        orderby b.End_Date ascending
                        select b;

            dgBookings.ItemsSource = query.ToList();
        }

        //method to search books in db
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string genre = cbxGenre.SelectedItem as string;
            DateTime startDate = (DateTime)DPStart.SelectedDate;
            DateTime endDate = (DateTime)DPEnd.SelectedDate;


         //make query to the db and show available books which match genre
         
            DateTime current = DateTime.Now;
            var freeBooks = db.BookTbls
            .Where(b => b.Genre == genre)
            .Where(b => !db.BookingTbls.Any(booking => booking.Book_Id == b.Book_Id &&
                                   ((booking.Start_Date <= startDate && booking.End_Date >= startDate)
                                    || (booking.Start_Date <= endDate && booking.End_Date >= endDate)
                                    || (booking.Start_Date >= startDate && booking.End_Date <= endDate)))); 


            if (genre == "All")
            {
                 freeBooks = db.BookTbls
            .Where(b => !db.BookingTbls.Any(booking => booking.Book_Id == b.Book_Id &&
                                   ((booking.Start_Date <= startDate && booking.End_Date >= startDate)
                                    || (booking.Start_Date <= endDate && booking.End_Date >= endDate)
                                    || (booking.Start_Date >= startDate && booking.End_Date <= endDate))));
            }
            lbxBooks.ItemsSource = freeBooks.ToList();
        }


        //method to activate btnsearch after validation of filling combobox and data
        private void Check_Date()
        {
            bool dateSelected = DPStart.SelectedDate.HasValue && DPEnd.SelectedDate.HasValue;
            if (cbxGenre.SelectedItem != null && dateSelected)
            {
                btnSearch.IsEnabled = true;
            }
            else { btnSearch.IsEnabled = false; }
        }


        //method to activate button book
        private void Check_Selection()
        {
            bool bookSelected = TblkSelected.IsHyphenationEnabled;
            if (TblkSelected.Text.Length > 0)
            {
                btnBook.IsEnabled = true;
            }
            else { btnSearch.IsEnabled = false; }
        }

        //update info in text block
        private void lbxBooks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //looking for a selected book
            BookTbl book = lbxBooks.SelectedItem as BookTbl;

            if (book != null)
            {
                // Update of text information about selected book
                TblkSelected.Text = book.Title +" "+book.Genre;
                btnBook.IsEnabled = true;
            }            
        }
        
        private void btnBook_Click(object sender, RoutedEventArgs e)
        {
            //Get selected book
            BookTbl book = lbxBooks.SelectedItem as BookTbl;
            if (book != null)
            {
                //add this book to db.BookingTbls and save changes
                BookingTbl booking = new BookingTbl
                {                    
                    Start_Date = DPStart.SelectedDate.Value,
                    End_Date = DPEnd.SelectedDate.Value,
                    Book_Id = book.Book_Id
                };

                //add to database
                db.BookingTbls.Add(booking);
                db.SaveChanges();

                //Reload bookings from db
                ShowBookings();

                //Clean all form input
                lbxBooks.SelectedIndex = -1;
                lbxBooks.ItemsSource = null;
                lbxBooks.Items.Clear();
                DPStart.Text = null;
                DPEnd.Text = null;
                cbxGenre.Text = "";
                TblkSelected.Text = null;

                //confirmation message to display
                string message = ($"Booking information \n\nBook ID : {book.Book_Id} \nTitle : {book.Title} \nGenre : {book.Genre} \nStart Date : {booking.Start_Date.ToString("dd/MM/yyyy")} \nEnd Date : {booking.End_Date.ToString("dd/MM/yyyy")}");
                MessageBox.Show(message);
               
            }
        }

        //method to delete bookings
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            BookingTbl booking = dgBookings.SelectedItem as BookingTbl;

            if (booking != null) {
                db.BookingTbls.Remove(booking);
                db.SaveChanges();
                // Reload bookings from db
                ShowBookings();
            }
        }

        //calling method check_date
        private void cbxGenre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Check_Date();
        }

        //calling method check_date
        private void DPStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Check_Date();
        }

        //calling method check_date
        private void DPEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Check_Date();
        }

        //method to activate button delete
        private void dgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BookingTbl booking = dgBookings.SelectedItem as BookingTbl;

            if (booking != null)
            {
                btnDelete.IsEnabled = true;
            }
            else
            {
                btnDelete.IsEnabled = false;
            }
        }

        private void DPEnd_CalendarOpened(object sender, RoutedEventArgs e)
        {
            DPEnd.DisplayDateStart = DPStart.SelectedDate.Value;
        }
    }
}
