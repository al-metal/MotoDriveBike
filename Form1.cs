﻿using NehouseLibrary;
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

        private void BtnUpdateMoto_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.login = tbLogin.Text;
            Properties.Settings.Default.password = tbPassword.Text;
            Properties.Settings.Default.Save();

            countDeleteTovar = 0;

            cookieB18 = nethouse.CookieNethouse(tbLogin.Text, tbPassword.Text);

            if (cookieB18.Count == 1)
            {
                MessageBox.Show("Логин или пароль введены не верно!", "Ошибка");
                return;
            }

            UpdateProducts(cookieB18, "https://bike18.ru/products/category/dorozhnye-bez-probega-po-rf");
            UpdateProducts(cookieB18, "https://bike18.ru/products/category/krossovye-bez-probega-po-rf");
            UpdateProducts(cookieB18, "https://bike18.ru/products/category/enduro-bez-probega-po-rf");
            UpdateProducts(cookieB18, "https://bike18.ru/products/category/motard-bez-probega-po-rf");
            UpdateProducts(cookieB18, "https://bike18.ru/products/category/sportivnye-bez-probega-po-rf");
            UpdateProducts(cookieB18, "https://bike18.ru/products/category/choppery-kruizery-bez-probega-po-rf");

            MessageBox.Show("Удалено товаров: " + countDeleteTovar);
        }

        private void UpdateProducts(CookieDictionary cookieB18, string url)
        {
            List<string> product = new List<string>();

            if (!url.Contains("?page=all"))
                url = url + "?page=all";

            otv = nethouse.getRequest(url);

            MatchCollection productsUrl = new Regex("(?<=<div class=\"product-item__link\"><a href=\").*?(?=\">)").Matches(otv);
            for (int i = 0; productsUrl.Count > i; i++)
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

                for (int y = 0; cartProductDB.Count > y; y++)
                {
                    searchProduct = false;
                    string productCart = cartProductDB[y].ToString();

                    string urlProductDB = new Regex("(?<=a href=\").*?(?=\")").Match(productCart).ToString().Trim();

                    otv = nethouse.getRequest(urlProductDB);
                    string articlDB = new Regex("(?<=Код товара:).*?(?=<br>)").Match(otv).ToString().Trim();

                    if (articlDB == productArticle)
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
        }
    }
}
