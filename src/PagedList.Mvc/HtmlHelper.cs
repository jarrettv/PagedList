﻿using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PagedList.Mvc
{
	///<summary>
	///	Extension methods for generating paging controls that can operate on instances of IPagedList.
	///</summary>
	public static class HtmlHelper
	{
		private static TagBuilder WrapInListItem(TagBuilder inner, params string[] classes)
		{
			var li = new TagBuilder("li");
			foreach (var @class in classes)
				li.AddCssClass(@class);
			li.InnerHtml = inner.ToString();
			return li;
		}

		private static TagBuilder First(IPagedList list, Func<int, string> generatePageUrl, string format)
		{
			const int targetPageNumber = 1;
			var first = new TagBuilder("a");
			first.SetInnerText(string.Format(format, targetPageNumber));

			if (list.IsFirstPage)
				return WrapInListItem(first, "PagedList-skipToFirst", "PagedList-disabled");

			first.Attributes["href"] = generatePageUrl(targetPageNumber);
			return WrapInListItem(first, "PagedList-skipToFirst");
		}

		private static TagBuilder Previous(IPagedList list, Func<int, string> generatePageUrl, string format)
		{
			var targetPageNumber = list.PageNumber - 1;
			var previous = new TagBuilder("a");
			previous.SetInnerText(string.Format(format, targetPageNumber));

			if (!list.HasPreviousPage)
				return WrapInListItem(previous, "PagedList-skipToPrevious", "PagedList-disabled");

			previous.Attributes["href"] = generatePageUrl(targetPageNumber);
			return WrapInListItem(previous, "PagedList-skipToPrevious");
		}

		private static TagBuilder Page(int i, IPagedList list, Func<int, string> generatePageUrl, string format)
		{
			return Page(i, list, generatePageUrl, (pageNumber => string.Format(format, pageNumber)));
		}

		private static TagBuilder Page(int i, IPagedList list, Func<int, string> generatePageUrl, Func<int, string> format)
		{
			var targetPageNumber = i;
			var page = new TagBuilder("a");
			page.SetInnerText(format(targetPageNumber));

			if (i == list.PageNumber)
				return WrapInListItem(page, "PagedList-skipToPage", "PagedList-currentPage", "PagedList-disabled");

			page.Attributes["href"] = generatePageUrl(targetPageNumber);
			return WrapInListItem(page, "PagedList-skipToPage");
		}

		private static TagBuilder Next(IPagedList list, Func<int, string> generatePageUrl, string format)
		{
			var targetPageNumber = list.PageNumber + 1;
			var next = new TagBuilder("a");
			next.SetInnerText(string.Format(format, targetPageNumber));

			if (!list.HasNextPage)
				return WrapInListItem(next, "PagedList-skipToNext", "PagedList-disabled");

			next.Attributes["href"] = generatePageUrl(targetPageNumber);
			return WrapInListItem(next, "PagedList-skipToNext");
		}

		private static TagBuilder Last(IPagedList list, Func<int, string> generatePageUrl, string format)
		{
			var targetPageNumber = list.PageCount;
			var last = new TagBuilder("a");
			last.SetInnerText(string.Format(format, targetPageNumber));

			if (list.IsLastPage)
				return WrapInListItem(last, "PagedList-skipToLast", "PagedList-disabled");

			last.Attributes["href"] = generatePageUrl(targetPageNumber);
			return WrapInListItem(last, "PagedList-skipToLast");
		}

		private static TagBuilder PageCountAndLocationText(IPagedList list, string format)
		{
			var text = new TagBuilder("span");
			text.SetInnerText(string.Format(format, list.PageNumber, list.PageCount));

			return WrapInListItem(text, "PagedList-pageCountAndLocation");
		}

		private static TagBuilder ItemSliceAndTotalText(IPagedList list, string format)
		{
			var text = new TagBuilder("span");
			text.SetInnerText(string.Format(format, list.FirstItemOnPage, list.LastItemOnPage, list.TotalItemCount));

			return WrapInListItem(text, "PagedList-pageCountAndLocation");
		}

		private static TagBuilder Ellipses(string format)
		{
			var text = new TagBuilder("span");
			text.SetInnerText(format); //TODO: fix so we can use &#8230;

			return WrapInListItem(text, "PagedList-ellipses");
		}

		///<summary>
		///	Displays a configurable paging control for instances of PagedList.
		///</summary>
		///<param name = "html">This method is meant to hook off HtmlHelper as an extension method.</param>
		///<param name = "list">The PagedList to use as the data source.</param>
		///<param name = "generatePageUrl">A function that takes the page number of the desired page and returns a URL-string that will load that page.</param>
		///<returns>Outputs the paging control HTML.</returns>
		public static MvcHtmlString PagedListPager(this System.Web.Mvc.HtmlHelper html,
												   IPagedList list,
												   Func<int, string> generatePageUrl)
		{
			return PagedListPager(html, list, generatePageUrl, new PagedListRenderOptions());
		}

		///<summary>
		///	Displays a configurable paging control for instances of PagedList.
		///</summary>
		///<param name = "html">This method is meant to hook off HtmlHelper as an extension method.</param>
		///<param name = "list">The PagedList to use as the data source.</param>
		///<param name = "generatePageUrl">A function that takes the page number  of the desired page and returns a URL-string that will load that page.</param>
		///<param name = "options">Formatting options.</param>
		///<returns>Outputs the paging control HTML.</returns>
		public static MvcHtmlString PagedListPager(this System.Web.Mvc.HtmlHelper html,
												   IPagedList list,
												   Func<int, string> generatePageUrl,
												   PagedListRenderOptions options)
		{
			var listItemLinks = new StringBuilder();

			//first
			if (options.DisplayLinkToFirstPage)
				listItemLinks.Append(First(list, generatePageUrl, options.LinkToFirstPageFormat));

			//previous
			if (options.DisplayLinkToPreviousPage)
				listItemLinks.Append(Previous(list, generatePageUrl, options.LinkToPreviousPageFormat));

			//text
			if (options.DisplayPageCountAndCurrentLocation)
				listItemLinks.Append(PageCountAndLocationText(list, options.PageCountAndCurrentLocationFormat));

			//text
			if (options.DisplayItemSliceAndTotal)
				listItemLinks.Append(ItemSliceAndTotalText(list, options.ItemSliceAndTotalFormat));

			//page
			if (options.DisplayLinkToIndividualPages)
			{
				//calculate start and end of range of page numbers
				var start = 1;
				var end = list.PageCount;
				if (options.MaximumPageNumbersToDisplay.HasValue && list.PageCount > options.MaximumPageNumbersToDisplay)
				{
					var maxPageNumbersToDisplay = options.MaximumPageNumbersToDisplay.Value;
					start = list.PageNumber - maxPageNumbersToDisplay / 2;
					if (start < 1)
						start = 1;
					end = maxPageNumbersToDisplay;
					if ((start + end - 1) > list.PageCount)
						start = list.PageCount - maxPageNumbersToDisplay + 1;
				}

				//if there are previous page numbers not displayed, show an ellipsis
				if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && start > 1)
					listItemLinks.Append(Ellipses(options.EllipsesFormat));

				foreach (var i in Enumerable.Range(start, end))
				{
					//show delimiter between page numbers
					if (i > start && !string.IsNullOrWhiteSpace(options.DelimiterBetweenPageNumbers))
						listItemLinks.Append(options.DelimiterBetweenPageNumbers);

					//show page number link
					listItemLinks.Append(options.FunctionToDisplayEachPageNumber == null
											 ? Page(i, list, generatePageUrl, options.LinkToIndividualPageFormat)
											 : Page(i, list, generatePageUrl, options.FunctionToDisplayEachPageNumber));
				}

				//if there are subsequent page numbers not displayed, show an ellipsis
				if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && (start + end - 1) < list.PageCount)
					listItemLinks.Append(Ellipses(options.EllipsesFormat));
			}

			//next
			if (options.DisplayLinkToNextPage)
				listItemLinks.Append(Next(list, generatePageUrl, options.LinkToNextPageFormat));

			//last
			if (options.DisplayLinkToLastPage)
				listItemLinks.Append(Last(list, generatePageUrl, options.LinkToLastPageFormat));

			var ul = new TagBuilder("ul")
						{
							InnerHtml = listItemLinks.ToString()
						};

			var outerDiv = new TagBuilder("div");
			outerDiv.AddCssClass("PagedList-pager");
			outerDiv.InnerHtml = ul.ToString();

			return new MvcHtmlString(outerDiv.ToString());
		}

		///<summary>
		/// Displays a configurable "Go To Page:" form for instances of PagedList.
		///</summary>
		///<param name="html">This method is meant to hook off HtmlHelper as an extension method.</param>
		///<param name="list">The PagedList to use as the data source.</param>
		///<param name="formAction">The URL this form should submit the GET request to.</param>
		///<returns>Outputs the "Go To Page:" form HTML.</returns>
		public static MvcHtmlString PagedListGoToPageForm(this System.Web.Mvc.HtmlHelper html,
														  IPagedList list,
														  string formAction)
		{
			return PagedListGoToPageForm(html, list, formAction, "page");
		}

		///<summary>
		/// Displays a configurable "Go To Page:" form for instances of PagedList.
		///</summary>
		///<param name="html">This method is meant to hook off HtmlHelper as an extension method.</param>
		///<param name="list">The PagedList to use as the data source.</param>
		///<param name="formAction">The URL this form should submit the GET request to.</param>
		///<param name="inputFieldName">The querystring key this form should submit the new page number as.</param>
		///<returns>Outputs the "Go To Page:" form HTML.</returns>
		public static MvcHtmlString PagedListGoToPageForm(this System.Web.Mvc.HtmlHelper html,
		                                                  IPagedList list,
		                                                  string formAction,
		                                                  string inputFieldName)
		{
			return PagedListGoToPageForm(html, list, formAction, new GoToFormRenderOptions(inputFieldName));
		}

		///<summary>
		/// Displays a configurable "Go To Page:" form for instances of PagedList.
		///</summary>
		///<param name="html">This method is meant to hook off HtmlHelper as an extension method.</param>
		///<param name="list">The PagedList to use as the data source.</param>
		///<param name="formAction">The URL this form should submit the GET request to.</param>
		///<param name="options">Formatting options.</param>
		///<returns>Outputs the "Go To Page:" form HTML.</returns>
		public static MvcHtmlString PagedListGoToPageForm(this System.Web.Mvc.HtmlHelper html,
		                                         IPagedList list,
		                                         string formAction,
		                                         GoToFormRenderOptions options)
		{
			var form = new TagBuilder("form");
			form.AddCssClass("PagedList-goToPage");
			form.Attributes.Add("action", formAction);
			form.Attributes.Add("method", "get");

			var fieldset = new TagBuilder("fieldset");

			var label = new TagBuilder("label");
			label.Attributes.Add("for", options.InputFieldName);
			label.SetInnerText(options.LabelFormat);

			var input = new TagBuilder("input");
			input.Attributes.Add("type", options.InputFieldType);
			input.Attributes.Add("name", options.InputFieldName);
			input.Attributes.Add("value", list.PageNumber.ToString());

			var submit = new TagBuilder("input");
			submit.Attributes.Add("type", "submit");
			submit.Attributes.Add("value", options.SubmitButtonFormat);

			fieldset.InnerHtml = label.ToString();
			fieldset.InnerHtml += input.ToString(TagRenderMode.SelfClosing);
			fieldset.InnerHtml += submit.ToString(TagRenderMode.SelfClosing);
			form.InnerHtml = fieldset.ToString();
			return new MvcHtmlString(form.ToString());
		}
	}
}