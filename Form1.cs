using NehouseLibrary;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using xNet.Net;

namespace MotoDriveBike
{
    public partial class Form1 : Form
    {
        CookieDictionary cookieB18 = new CookieDictionary();
        nethouse nethouse = new nethouse();
        
        string otv;
        int countDeleteTovar;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tbLogin.Text = Properties.Settings.Default.login;
            tbPassword.Text = Properties.Settings.Default.password;
        }

        private void btnUpdateMoto_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.login = tbLogin.Text;
            Properties.Settings.Default.password = tbPassword.Text;
            Properties.Settings.Default.Save();

            List<string> product = new List<string>();
            countDeleteTovar = 0;

            cookieB18 = nethouse.CookieNethouse(tbLogin.Text, tbPassword.Text);

            if (cookieB18.Count == 1)
            {
                MessageBox.Show("Логин или пароль введены не верно!", "Ошибка");
                return;
            }

            otv = nethouse.getRequest("https://bike18.ru/products/category/dorozhnye-bez-probega-po-rf?page=all");

            MatchCollection productsUrl = new Regex("(?<=<div class=\"product-item__link\"><a href=\").*?(?=\">)").Matches(otv);
            for(int i = 0; productsUrl.Count > i; i++)
            {
                string urlProduct = productsUrl[i].ToString();
                product = nethouse.GetProductList(cookieB18, urlProduct);

                string productName = product[4];
                string productArticle = product[6];

                char firstCharArticl = productArticle[0];

                bool firstChar = false;
                bool searchProduct = false;

                if (firstCharArticl == 'D')
                    firstChar = true;
                else if (firstCharArticl == 'd')
                    firstChar = true;

                if (!firstChar)
                    continue;

                productArticle = productArticle.Remove(0, 1);

                otv = nethouse.getRequest("https://www.drivebike.ru/catalogsearch/result/?cat=0&q=" + productArticle);

                MatchCollection cartProductDB = new Regex("class=\"item-brandName\">[\\w\\W]*?</a>").Matches(otv);

                for(int y = 0; cartProductDB.Count > y; y++)
                {
                    searchProduct = false;
                    string productCart = cartProductDB[y].ToString();

                    string firstNameDB = new Regex("(?<=class=\"item-brandName\">)[\\w\\W]*?(?=</div>)").Match(productCart).ToString().Trim();
                    string lastNameDB = new Regex("(?<=title=\").*?(?=\">)").Match(productCart).ToString().Trim();

                    string nameDB = firstNameDB + " " + lastNameDB;

                    if(nameDB == productName)
                    {
                        searchProduct = true;
                        break;
                    }
                }

                if (!searchProduct)
                {
                    nethouse.DeleteProduct(cookieB18, product);
                    countDeleteTovar++;
                }
            }
            MessageBox.Show("Удалено товаров: " + countDeleteTovar);
        }
    }
}
