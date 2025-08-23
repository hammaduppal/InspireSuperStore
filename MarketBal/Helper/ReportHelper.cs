namespace MarketBal.Helper
{
    public class ReportHelper
    {
        public static string GetCustomCSS()
        {
            var css = $@"

                .container {{width: 100%; max-width: 1200px; margin: 0 auto; padding: 0 15px; }}
                .row {{display: flex; flex-wrap: wrap; margin: 0 -15px; }}
                .col {{flex: 1 0 0%; padding: 0 15px; }}
                .col-1 {{flex: 0 0 8.3333%; max-width: 8.3333%; padding: 0 15px; }}
                .col-2 {{flex: 0 0 16.6667%; max-width: 16.6667%; padding: 0 15px; }}
                .col-3 {{flex: 0 0 25%; max-width: 25%; padding: 0 15px; }}
                .col-4 {{flex: 0 0 33.3333%; max-width: 33.3333%; padding: 0 15px; }}
                .col-5 {{flex: 0 0 41.6667%; max-width: 41.6667%; padding: 0 15px; }}
                .col-6 {{flex: 0 0 50%; max-width: 50%; padding: 0 15px; }}
                .col-7 {{flex: 0 0 58.3333%; max-width: 58.3333%; padding: 0 15px; }}
                .col-8 {{flex: 0 0 66.6667%; max-width: 66.6667%; padding: 0 15px; }}
                .col-9 {{flex: 0 0 75%; max-width: 75%; padding: 0 15px; }}
                .col-10 {{flex: 0 0 83.3333%; max-width: 83.3333%; padding: 0 15px; }}
                .col-11 {{flex: 0 0 91.6667%; max-width: 91.6667%; padding: 0 15px; }}
                .col-12 {{flex: 0 0 100%; max-width: 100%; padding: 0 15px; }}

                body {{font-family: Arial, sans-serif; font-size: 16px; line-height: 1.5; color: #212529; }}
                h1, .h1 {{font-size: 2.5rem; font-weight: 700; }}
                h2, .h2 {{font-size: 2rem; font-weight: 700; }}
                h3, .h3 {{font-size: 1.75rem; font-weight: 700; }}
                h4, .h4 {{font-size: 1.5rem; font-weight: 600; }}
                h5, .h5 {{font-size: 1.25rem; font-weight: 600; }}
                h6, .h6 {{font-size: 1rem; font-weight: 600; }}
                p {{margin-top: 0; margin-bottom: 1rem; }}
                .lead {{font-size: 1.25rem; font-weight: 300; }}
                small {{font-size: 0.875rem; }}
                strong {{font-weight: 700; }}
                em {{font-style: italic; }}


                ul, ol {{padding-left: 2rem; margin-bottom: 1rem; }}
                li {{margin-bottom: .5rem; }}
                .list-unstyled {{padding-left: 0; list-style: none; }}
                .list-inline {{padding-left: 0; list-style: none; display: flex; gap: .5rem; }}
                .list-inline li {{display: inline-block; margin-bottom: 0; }}


                .table {{width: 100%; margin-bottom: 1rem; color: #212529; border-collapse: collapse; }}
                .table th, .table td {{padding: .75rem; vertical-align: top; border: 1px solid #dee2e6; }}
                .table thead th {{vertical-align: bottom; border-bottom: 2px solid #dee2e6; }}
                .table-hover tbody tr:hover {{background-color: rgba(0,0,0,.075); }}
                .table-striped tbody tr:nth-of-type(odd) {{background-color: rgba(0,0,0,.05); }}


                .btn {{display: inline-block; font-weight: 400; text-align: center; vertical-align: middle; padding: .375rem .75rem; font-size: 1rem; line-height: 1.5; border-radius: .25rem; cursor: pointer; transition: all .15s; }}
                .btn-primary {{color: #fff; background-color: #0d6efd; border: 1px solid #0d6efd; }}
                .btn-secondary {{color: #fff; background-color: #6c757d; border: 1px solid #6c757d; }}
                .btn-sm {{padding: .25rem .5rem; font-size: .875rem; border-radius: .2rem; }}


                .card {{position: relative; display: flex; flex-direction: column; min-width: 0; word-wrap: break-word; background-color: #fff; border: 1px solid rgba(0,0,0,.125); border-radius: .25rem; }}
                .card-body {{flex: 1 1 auto; padding: 1rem; }}


                .m-0 {{margin: 0 !important; }}
                .mt-0 {{margin-top: 0 !important; }}
                .mb-0 {{margin-bottom: 0 !important; }}
                .p-0 {{padding: 0 !important; }}
                .pt-0 {{padding-top: 0 !important; }}
                .pb-0 {{padding-bottom: 0 !important; }}
                .m-1 {{margin: .25rem !important; }}
                .p-1 {{padding: .25rem !important; }}
                .m-2 {{margin: .5rem !important; }}
                .p-2 {{padding: .5rem !important; }}
                .m-3 {{margin: 1rem !important; }}
                .p-3 {{padding: 1rem !important; }}
                .m-4 {{margin: 1.5rem !important; }}
                .p-4 {{padding: 1.5rem !important; }}
                .m-5 {{margin: 3rem !important; }}
                .p-5 {{padding: 3rem !important; }}

                .text-start {{text-align: left !important; }}
                .text-center {{text-align: center !important; }}
                .text-end {{text-align: right !important; }}
                .text-muted {{color: #6c757d !important; }}
                .text-success {{color: #198754 !important; }}
                .text-danger {{color: #dc3545 !important; }}
                .text-warning {{color: #ffc107 !important; }}
                .font-weight-bold {{font-weight: 700 !important; }}
                .font-weight-semibold {{font-weight: 600 !important; }}



                           ";
            return css;
        }
        
    }
    
}
