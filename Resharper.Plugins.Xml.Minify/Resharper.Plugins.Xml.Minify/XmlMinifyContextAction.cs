using System;
using System.Text.RegularExpressions;
using JetBrains.Application.Progress;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Xml.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Xml.Tree;
using JetBrains.TextControl;
using JetBrains.Util;
using JetBrains.Util.dataStructures.TypedIntrinsics;

namespace Resharper.Plugins.Xml.Minify
{
    [ContextAction(Name = "Minify", Group = "XML", Description = "Minifies the XML file by removing unnecessary spaces, tabs and carriage returns.", Priority = 0)]
    public class XmlMinifyContextAction : BulbItemImpl, IContextAction
    {
        protected XmlContextActionDataProvider DataProvider { get; private set; }

        public XmlMinifyContextAction(XmlContextActionDataProvider provider)
        {
            DataProvider = provider;
        }

        public override string Text { get { return "Minify"; } }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            ITreeNode selectedElement = DataProvider.SelectedElement;
            if (selectedElement != null)
            {
                IPsiSourceFile sourceFile = selectedElement.GetSourceFile();
                if (sourceFile != null)
                {
                    IDocument document = sourceFile.Document;
                    string minifiedDocumentTest = GetMinifiedText(document.GetText());
                    document.ReplaceText(document.DocumentRange, minifiedDocumentTest);
                }
            }

            return null;
        }

        private string GetMinifiedText(string input)
        {
            // Remove carriage returns, tabs and whitespaces
            string output = Regex.Replace(input, @"\n|\t", " ");
            output = Regex.Replace(output, @">\s+<", "><").Trim();
            output = Regex.Replace(output, @"\s{2,}", " ");

            // Remove comments 
            output = Regex.Replace(output, "<!--.*?-->", String.Empty, RegexOptions.Singleline);

            return output;
        }

        public bool IsAvailable(IUserDataHolder cache)
        {
            try
            {
                IXmlTag tag = DataProvider.FindNodeAtCaret<IXmlTag>();

                // check if we have the XML root element
                // and the document has multiple lines
                if (tag != null && !tag.IsEmptyTag && tag.PathToRoot().IsNullOrEmpty())
                {
                    ITreeNode selectedElement = DataProvider.SelectedElement;
                    if (selectedElement != null)
                    {
                        IPsiSourceFile sourceFile = selectedElement.GetSourceFile();
                        if (sourceFile != null)
                        {
                            Int32<DocLine> lineCount = sourceFile.Document.GetLineCount();
                            bool isSingleLineDocument = lineCount == Int32<DocLine>.I;
                            return !isSingleLineDocument;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
