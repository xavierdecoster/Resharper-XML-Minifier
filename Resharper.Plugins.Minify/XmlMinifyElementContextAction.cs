using System;
using JetBrains.Application.Progress;
using JetBrains.DocumentManagers;
using JetBrains.DocumentModel;
using JetBrains.DocumentModel.Transactions;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.Xml.Bulbs;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Resx.Utils;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Xml.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace Resharper.Plugins.Minify
{
    [ContextAction(Name = "Resharper.Plugins.Xml.XmlMinifyElementContextAction", Group = "XML",
        Description = "Minifies the selected XML element by removing unnecessary spaces, tabs and carriage returns.", Priority = 0)]
    public class XmlMinifyElementContextAction : BulbItemImpl, IContextAction
    {
        protected IMinifier Minifier { get; private set; }
        protected XmlContextActionDataProvider DataProvider { get; private set; }

        public XmlMinifyElementContextAction(XmlContextActionDataProvider provider)
        {
            DataProvider = provider;
            Minifier = new XmlMinifier();
        }

        public override string Text
        {
            get
            {
                IXmlTag tag = DataProvider.FindNodeAtCaret<IXmlTag>();
                if (tag != null)
                {
                    if (tag.PathToRoot().IsNullOrEmpty())
                        return "Minify file";

                    return string.Format("Minify element '{0}'", tag.GetName().GetText());
                }

                return "Minify element";
            }
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            if (SelectedXmlTag != null)
            {
                IPsiSourceFile sourceFile = SelectedXmlTag.GetSourceFile();
                TreeTextRange valueRange = SelectedXmlTag.GetValueRange();

                TextRange textRange = new TextRange(valueRange.StartOffset.Offset, valueRange.EndOffset.Offset);
                if (sourceFile != null)
                {
                    try
                    {
                        sourceFile.Document.DeleteText(textRange);
                        sourceFile.Document.InsertText(textRange.StartOffset, SelectedXmlTagMinifiedInnerText);
                    }
                    catch
                    {
                        // nomnom...
                    }
                }
            }

            return null;
        }

        protected IXmlTag SelectedXmlTag { get; private set; }
        protected string SelectedXmlTagInnerText { get; private set; }
        protected string SelectedXmlTagMinifiedInnerText { get; private set; }

        public bool IsAvailable(IUserDataHolder cache)
        {
            SelectedXmlTag = DataProvider.FindNodeAtCaret<IXmlTag>();

            if (SelectedXmlTag != null && !SelectedXmlTag.IsEmptyTag && !SelectedXmlTag.Children().CountIs(0))
            {
                // TODO: remove the next block once you know how to auto-save the doc after executing the action
                if (!SelectedXmlTag.PathToRoot().CountIs(0))
                    return false;
                //end

                SelectedXmlTagInnerText = SelectedXmlTag.InnerText;
                SelectedXmlTagMinifiedInnerText = Minifier.Minify(SelectedXmlTagInnerText);
                bool isAvailable = !string.Equals(SelectedXmlTagInnerText, SelectedXmlTagMinifiedInnerText);

                return isAvailable;
            }

            return false;
        }
    }
}
