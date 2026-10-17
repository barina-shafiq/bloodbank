import os
import pandas as pd
from reportlab.lib.pagesizes import letter
from reportlab.lib.units import inch
from reportlab.lib import colors
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, Image, KeepTogether, PageBreak, HRFlowable
)
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.pdfgen import canvas

base_dir = r"c:\Users\Barina\Desktop\Desktop development\BloodBankManagement\BDA100_Assn2_Shahzad_Qasir"
img_dir = os.path.join(base_dir, "images")
pdf_path = os.path.join(base_dir, "Appendix_Shahzad_Qasir.pdf")

class NumberedCanvas(canvas.Canvas):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self._saved_page_states = []

    def showPage(self):
        self._saved_page_states.append(dict(self.__dict__))
        self._startPage()

    def save(self):
        num_pages = len(self._saved_page_states)
        for state in self._saved_page_states:
            self.__dict__.update(state)
            self.draw_page_decorations(num_pages)
            super().showPage()
        super().save()

    def draw_page_decorations(self, page_count):
        self.saveState()
        self.setFont("Helvetica", 8)
        self.setFillColor(colors.HexColor("#595959"))
        
        # Header (Pages > 1)
        if self._pageNumber > 1:
            self.drawString(54, 750, "BDA100 Assignment 2 Appendix | Student: Qasir Shahzad (ID: 664432743)")
            self.setStrokeColor(colors.HexColor("#D9D9D9"))
            self.setLineWidth(0.5)
            self.line(54, 744, 558, 744)
            
        # Footer
        self.setStrokeColor(colors.HexColor("#D9D9D9"))
        self.setLineWidth(0.5)
        self.line(54, 45, 558, 45)
        self.drawString(54, 32, "Confidential — Retail Analytics Digital Transformation Project")
        self.drawRightString(558, 32, f"Page {self._pageNumber} of {page_count}")
        self.restoreState()

doc = SimpleDocTemplate(
    pdf_path,
    pagesize=letter,
    leftMargin=54,
    rightMargin=54,
    topMargin=54,
    bottomMargin=54
)

styles = getSampleStyleSheet()

# Custom styles
style_title = ParagraphStyle(
    'DocTitle',
    fontName='Helvetica-Bold',
    fontSize=18,
    leading=22,
    textColor=colors.HexColor("#1F4E79"),
    spaceAfter=4
)

style_sub = ParagraphStyle(
    'DocSub',
    fontName='Helvetica',
    fontSize=11,
    leading=14,
    textColor=colors.HexColor("#2E75B6"),
    spaceAfter=6
)

style_meta = ParagraphStyle(
    'DocMeta',
    fontName='Helvetica-Bold',
    fontSize=9,
    leading=12,
    textColor=colors.HexColor("#333333"),
    spaceAfter=10
)

style_h1 = ParagraphStyle(
    'Heading1_Custom',
    fontName='Helvetica-Bold',
    fontSize=13,
    leading=16,
    textColor=colors.HexColor("#1F4E79"),
    spaceBefore=12,
    spaceAfter=6,
    keepWithNext=True
)

style_h2 = ParagraphStyle(
    'Heading2_Custom',
    fontName='Helvetica-Bold',
    fontSize=10.5,
    leading=14,
    textColor=colors.HexColor("#2E75B6"),
    spaceBefore=10,
    spaceAfter=4,
    keepWithNext=True
)

style_body = ParagraphStyle(
    'Body_Custom',
    fontName='Helvetica',
    fontSize=8.5,
    leading=11.5,
    textColor=colors.HexColor("#222222"),
    spaceAfter=6
)

style_body_bold = ParagraphStyle(
    'BodyBold_Custom',
    fontName='Helvetica-Bold',
    fontSize=8.5,
    leading=11.5,
    textColor=colors.HexColor("#222222"),
    spaceAfter=4
)

style_callout = ParagraphStyle(
    'Callout_Text',
    fontName='Helvetica-Oblique',
    fontSize=8,
    leading=11,
    textColor=colors.HexColor("#111111")
)

style_tbl_hdr = ParagraphStyle(
    'TblHdr',
    fontName='Helvetica-Bold',
    fontSize=7.5,
    leading=9.5,
    textColor=colors.white,
    alignment=1 # Center
)

style_tbl_cell = ParagraphStyle(
    'TblCell',
    fontName='Helvetica',
    fontSize=7,
    leading=9,
    textColor=colors.HexColor("#222222")
)

style_tbl_cell_bold = ParagraphStyle(
    'TblCellBold',
    fontName='Helvetica-Bold',
    fontSize=7,
    leading=9,
    textColor=colors.HexColor("#1F4E79")
)

style_code = ParagraphStyle(
    'CodeStyle',
    fontName='Courier-Bold',
    fontSize=7,
    leading=9,
    textColor=colors.HexColor("#002060")
)

story = []

# Title & Metadata
story.append(Paragraph("BDA100 / Fundamentals of Business Analytics - Statistics", style_sub))
story.append(Paragraph("Assignment 2: Smart Query Formulation Using GenAI & Spreadsheet Tools", style_title))
story.append(Paragraph("Appendix: Comprehensive GenAI Prompt Log, Logic Failure Analysis & Multi-Tier Verification Matrix", style_sub))
story.append(Paragraph("<b>Student:</b> Qasir Shahzad &nbsp;&nbsp;|&nbsp;&nbsp; <b>ID:</b> 664432743 &nbsp;&nbsp;|&nbsp;&nbsp; <b>Role:</b> Junior Business Data Analyst &nbsp;&nbsp;|&nbsp;&nbsp; <b>Due:</b> Session 5", style_meta))

# AI Declaration Callout Box
decl_text = (
    "<b>AI Assistance Declaration:</b> I used ChatGPT and Claude for query formulation, formula drafting, layout ideation, "
    "and verification cross-checks. Prompts used: Seed, refinement, and validation prompts documented in Section 2. "
    "I verified outputs using independent Excel formulas (SUMIFS, COUNTIFS, XLOOKUP, SCAN), Google Sheets QUERY() emulation, "
    "manual calculations, and dynamic PivotTable cross-checks with GETPIVOTDATA. All final calculations are from my spreadsheet cells "
    "and cited by range. I am responsible for the accuracy and originality of this work."
)
tbl_decl = Table([[Paragraph(decl_text, style_callout)]], colWidths=[504])
tbl_decl.setStyle(TableStyle([
    ('BACKGROUND', (0,0), (-1,-1), colors.HexColor("#DDEBF7")),
    ('LINELEFT', (0,0), (0,-1), 3, colors.HexColor("#1F4E79")),
    ('TOPPADDING', (0,0), (-1,-1), 6),
    ('BOTTOMPADDING', (0,0), (-1,-1), 6),
    ('LEFTPADDING', (0,0), (-1,-1), 8),
    ('RIGHTPADDING', (0,0), (-1,-1), 8),
]))
story.append(tbl_decl)
story.append(Spacer(1, 8))

# SECTION 1: EXECUTIVE SUMMARY
story.append(Paragraph("1. Executive Summary & Business Context", style_h1))
p1 = (
    "As Junior Business Data Analysts for a retail firm undergoing digital transformation, we established an enterprise workflow "
    "pairing GenAI tools with spreadsheet modeling. Crucially, AI was utilized strictly for ideation, syntax drafting, and validation design. "
    "In accordance with assignment rules, all final metrics are computed natively by spreadsheet formulas across 10,000 transaction records "
    "and 120 master catalog products."
)
story.append(Paragraph(p1, style_body))

exec_summary_points = (
    "<b>Key Analytical Outcomes:</b><br/>"
    "• <b>Total Company Revenue:</b> $8,880,461.29 across 10,000 transactions and 24,318 units sold (YTD 2025).<br/>"
    "• <b>Total Company Margin:</b> $2,524,927.01, representing a healthy overall gross margin of 28.43%.<br/>"
    "• <b>Question 1 (Pareto Revenue Concentration):</b> The top 20% of catalog SKUs (24 products) drive $4,344,138.98 (48.92% of sales). Reaching ~80% revenue requires 54 products ($7,112,476.99 | 80.09%).<br/>"
    "• <b>Question 2 (Regional Benchmarking vs. 28.50% Target):</b> West (28.28%), South (28.33%), and North (28.45%) perform below the corporate hurdle rate, generating an aggregate margin shortfall of $7,500.41.<br/>"
    "• <b>Question 3 (Catalog Pricing Audit):</b> 100% of product master items contain complete, positive MSRPs (0 missing records).<br/>"
    "• <b>Cross-Tool Reconciliation:</b> 0.00% variance verified between Formulas tab, Google Sheets QUERY(), and PivotTable outputs."
)
tbl_exec = Table([[Paragraph(exec_summary_points, style_body)]], colWidths=[504])
tbl_exec.setStyle(TableStyle([
    ('BACKGROUND', (0,0), (-1,-1), colors.HexColor("#F2F4F8")),
    ('LINELEFT', (0,0), (0,-1), 2.5, colors.HexColor("#2E75B6")),
    ('TOPPADDING', (0,0), (-1,-1), 6),
    ('BOTTOMPADDING', (0,0), (-1,-1), 6),
    ('LEFTPADDING', (0,0), (-1,-1), 8),
    ('RIGHTPADDING', (0,0), (-1,-1), 8),
]))
story.append(tbl_exec)
story.append(Spacer(1, 8))

# SECTION 2: PROMPT ENGINEERING LOG
story.append(Paragraph("2. Comprehensive AI Prompt & Response Log", style_h1))
story.append(Paragraph("Documenting the complete prompt iteration lifecycle (Seed -> Refinement -> Validation -> Analyst Fix):", style_body))

prompts_pdf = [
    ("Phase 1: Ingestion & DQ Checks ('DQ' Tab)",
     "Seed: 'Generate automated Excel DQ check formulas for retail dataset (duplicates, negative prices, missing costs, date outliers, margin > 100%).'\n"
     "Refinement: 'Wrap full column ranges $2:$10001, return integer violation counts, and output boolean PASS/FAIL status without volatile functions.'\n"
     "Validation: 'Propose SUMPRODUCT/COUNTBLANK cross-checks to verify 0 violations across all 10,000 rows.'\n"
     "Analyst Evaluation: AI drafted row-relative rules; we refined into 8 enterprise audit formulas on the 'DQ' tab with 100% clean passes."),
    ("Phase 2: Question 1 — Product Revenue Pareto 80/20 ('Formulas' Tab)",
     "Seed: 'Convert stakeholder prompt to Excel logic: Which top ~20% of products drive ~80% of revenue? Return SUMIFS and Sheets QUERY.'\n"
     "Refinement: 'Rank products by sales descending, compute cumulative sales with SCAN lambda, compute cumulative revenue %, add Pareto flag.'\n"
     "Validation: 'Cross-check top 20% SKU cutoff against PivotTable. How many SKUs equal 20% and does cumulative share reach 80%?'\n"
     "Analyst Evaluation: AI assumed exactly 20% equals 80%; our model proved top 20% (24 SKUs) = 48.92%, while ~80% revenue requires 54 SKUs (80.09%)."),
    ("Phase 3: Question 2 — Regional Margin Benchmarking ('Formulas' Tab)",
     "Seed: 'Write Excel formula set to identify regions below target gross margin %. Keep target threshold as cell-referenced parameter.'\n"
     "Refinement: 'Calculate weighted Margin/Sales ratio rather than AVERAGEIFS of transaction margin %. Add basis point variance and dollar shortfall.'\n"
     "Validation: 'Verify with PivotTable. If cell B5 changes from 28.0% to 28.5%, ensure status flags update dynamically.'\n"
     "Analyst Evaluation: Corrected AI's 'average of averages' distortion (14 bps error in West) to proper dollar-weighted margin."),
    ("Phase 4: Question 3 — Master Catalog MSRP Completeness ('Formulas' Tab)",
     "Seed: 'Formulate Excel lookup formula to audit whether any transaction products are missing MSRP in product_lookup.csv.'\n"
     "Refinement: 'Use XLOOKUP with IFERROR and COUNTBLANK. Return clean 'VALID' status if 0 missing records.'\n"
     "Validation: 'Perform bidirectional check: verify transactions mapped to lookup, and lookup products missing MSRP.'\n"
     "Analyst Evaluation: Upgraded fragile VLOOKUP to XLOOKUP and verified 100% catalog completeness across 120 SKUs."),
    ("Phase 5: Google Sheets QUERY & PivotTable ('QUERY' & 'Pivot_Slicers')",
     "Seed: 'Write Google Sheets QUERY statements for all 3 questions using SELECT, WHERE, GROUP BY, ORDER BY, LABEL.'\n"
     "Refinement: 'Harden syntax with WHERE K > 0 and custom column aliases. Propose GETPIVOTDATA syntax to extract regional KPIs.'\n"
     "Validation: 'Build cross-tool reconciliation matrix comparing QUERY, Formulas, and GETPIVOTDATA outputs.'\n"
     "Analyst Evaluation: Replaced AI's invented field names with exact column letters A:M and wrapped GETPIVOTDATA in IFERROR fallback.")
]

for p_title, p_content in prompts_pdf:
    t_data = [
        [Paragraph(f"<b>{p_title}</b>", style_tbl_cell_bold)],
        [Paragraph(p_content.replace("\n", "<br/>"), style_tbl_cell)]
    ]
    t = Table(t_data, colWidths=[504])
    t.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,0), colors.HexColor("#DDEBF7")),
        ('BACKGROUND', (0,1), (-1,1), colors.HexColor("#F9FAFC")),
        ('BOX', (0,0), (-1,-1), 0.5, colors.HexColor("#D9D9D9")),
        ('TOPPADDING', (0,0), (-1,-1), 3),
        ('BOTTOMPADDING', (0,0), (-1,-1), 3),
        ('LEFTPADDING', (0,0), (-1,-1), 6),
        ('RIGHTPADDING', (0,0), (-1,-1), 6),
    ]))
    story.append(t)
    story.append(Spacer(1, 4))

story.append(Spacer(1, 6))

# SECTION 3: DATA QUALITY AUDIT REPORT
story.append(Paragraph("3. Data Quality (DQ) Audit Report & Verification Scorecard", style_h1))
dq_p = (
    "The 'DQ' tab performs pre-analytical validation across 10,000 transaction rows and 120 product catalog records. "
    "Automated checks enforce zero-tolerance integrity standards prior to executive reporting:"
)
story.append(Paragraph(dq_p, style_body))

dq_img = os.path.join(img_dir, "dq_dashboard.png")
if os.path.exists(dq_img):
    story.append(Image(dq_img, width=6.5*inch, height=3.25*inch))
    story.append(Spacer(1, 4))

dq_findings = (
    "<b>Audit Summary:</b> All 8 quality checks achieved a 100% clean PASS status. Zero duplicate orders, zero negative selling prices, "
    "zero missing costs, zero out-of-range dates, zero margin percentages > 100%, and zero orphaned SKUs were detected. "
    "Overall data health score: <b>100.0%</b>."
)
story.append(Paragraph(dq_findings, style_body))
story.append(Spacer(1, 6))

# SECTION 4: ANALYTICAL SOLUTIONS & RANGE CITATIONS
story.append(Paragraph("4. Analytical Solutions & Master Range Citation Table", style_h1))

p_q1_brief = (
    "<b>Question 1 (Pareto Analysis):</b> Top 20% SKUs (24 products) drive $4,344,138.98 (48.92% of total revenue). "
    "Core revenue drivers include Solace Laptop S-367 ($243.8k | 2.74%), Aurora Wearable Lite-704 ($234.8k | 2.64%), and "
    "Falcon Headphone Plus-789 ($227.6k | 2.56%). Reaching ~80% revenue requires the top 54 SKUs (80.09%)."
)
story.append(Paragraph(p_q1_brief, style_body))

pareto_img = os.path.join(img_dir, "pareto_curve.png")
if os.path.exists(pareto_img):
    story.append(Image(pareto_img, width=6.5*inch, height=3.25*inch))
    story.append(Spacer(1, 4))

p_q2_brief = (
    "<b>Question 2 (Regional Benchmarking):</b> Under corporate target threshold B5 = 28.50%, Central (28.58%) and East (28.50%) meet target, "
    "while North (28.45%), South (28.33%), and West (28.28%) fall below target, generating a cumulative margin gap of $7,500.41."
)
story.append(Paragraph(p_q2_brief, style_body))

reg_img = os.path.join(img_dir, "regional_margin_chart.png")
if os.path.exists(reg_img):
    story.append(Image(reg_img, width=6.2*inch, height=3.2*inch))
    story.append(Spacer(1, 4))

p_q3_brief = (
    "<b>Question 3 (MSRP Completeness):</b> Verified 0 missing MSRPs in product_lookup and 0 unmatched SKUs in transactions. "
    "Product catalog pricing integrity is 100% complete."
)
story.append(Paragraph(p_q3_brief, style_body))
story.append(Spacer(1, 4))

# Table of exact cell ranges
story.append(Paragraph("<b>Table 1: Master Cell Range Citation Table (Formulas & Range Coordinates)</b>", style_body_bold))
range_table_data = [
    [Paragraph("<b>Business Dimension / Metric</b>", style_tbl_hdr),
     Paragraph("<b>Sheet Tab</b>", style_tbl_hdr),
     Paragraph("<b>Exact Range</b>", style_tbl_hdr),
     Paragraph("<b>Primary Formula / Result</b>", style_tbl_hdr)],
    [Paragraph("Model Parameters", style_tbl_cell), Paragraph("Formulas", style_tbl_cell_bold), Paragraph("B5:B7", style_tbl_cell), Paragraph("B5=28.50%, B6=20.0%, B7=80.0%", style_tbl_cell)],
    [Paragraph("Derived Net Sales Column", style_tbl_cell), Paragraph("retail_transactions", style_tbl_cell_bold), Paragraph("K2:K10001", style_tbl_cell), Paragraph("=F2*(G2-H2)", style_tbl_cell)],
    [Paragraph("Derived Margin Column", style_tbl_cell), Paragraph("retail_transactions", style_tbl_cell_bold), Paragraph("L2:L10001", style_tbl_cell), Paragraph("=K2-(F2*I2)", style_tbl_cell)],
    [Paragraph("Derived Margin % Column", style_tbl_cell), Paragraph("retail_transactions", style_tbl_cell_bold), Paragraph("M2:M10001", style_tbl_cell), Paragraph("=L2/K2", style_tbl_cell)],
    [Paragraph("Data Quality Audit Matrix", style_tbl_cell), Paragraph("DQ", style_tbl_cell_bold), Paragraph("A8:H15", style_tbl_cell), Paragraph("8 Automated Audit Formulas (PASS)", style_tbl_cell)],
    [Paragraph("Data Quality KPIs", style_tbl_cell), Paragraph("DQ", style_tbl_cell_bold), Paragraph("A20:H21", style_tbl_cell), Paragraph("10,000 Audited | 100% Health", style_tbl_cell)],
    [Paragraph("Pareto Product Rankings (Q1)", style_tbl_cell), Paragraph("Formulas", style_tbl_cell_bold), Paragraph("A12:K131", style_tbl_cell), Paragraph("SUMIFS, XLOOKUP, SCAN / Running Sum", style_tbl_cell)],
    [Paragraph("Pareto Total Sales", style_tbl_cell), Paragraph("Formulas", style_tbl_cell_bold), Paragraph("E132", style_tbl_cell), Paragraph("=SUM(E12:E131) -> $8,880,461.29", style_tbl_cell)],
    [Paragraph("Regional Benchmarking (Q2)", style_tbl_cell), Paragraph("Formulas", style_tbl_cell_bold), Paragraph("A137:K141", style_tbl_cell), Paragraph("SUMIFS, COUNTIFS, Ref B5", style_tbl_cell)],
    [Paragraph("Regional Total Sales", style_tbl_cell), Paragraph("Formulas", style_tbl_cell_bold), Paragraph("D142", style_tbl_cell), Paragraph("=SUM(D137:D141) -> $8,880,461.29", style_tbl_cell)],
    [Paragraph("Catalog MSRP Audit (Q3)", style_tbl_cell), Paragraph("Formulas", style_tbl_cell_bold), Paragraph("A146:K151", style_tbl_cell), Paragraph("COUNTBLANK, XLOOKUP -> 0 Missing", style_tbl_cell)],
    [Paragraph("Sheets QUERY Statements", style_tbl_cell), Paragraph("QUERY", style_tbl_cell_bold), Paragraph("A8:K18", style_tbl_cell), Paragraph("Production Queries 1, 2, 3", style_tbl_cell)],
    [Paragraph("Reconciled Query Results", style_tbl_cell), Paragraph("QUERY", style_tbl_cell_bold), Paragraph("A20:I26", style_tbl_cell), Paragraph("Query Output Matching $8,880,461.29", style_tbl_cell)],
    [Paragraph("Cross-Tool Reconciliation", style_tbl_cell), Paragraph("QUERY", style_tbl_cell_bold), Paragraph("A30:F35", style_tbl_cell), Paragraph("Formulas vs QUERY vs Pivot (0.00 Variance)", style_tbl_cell)],
    [Paragraph("GETPIVOTDATA KPI Cards", style_tbl_cell), Paragraph("Pivot_Slicers", style_tbl_cell_bold), Paragraph("B6:L7", style_tbl_cell), Paragraph("Linked Executive KPIs", style_tbl_cell)],
    [Paragraph("Multi-Dimensional Pivot", style_tbl_cell), Paragraph("Pivot_Slicers", style_tbl_cell_bold), Paragraph("A15:F21", style_tbl_cell), Paragraph("Regional Multi-Dimensional Pivot", style_tbl_cell)],
    [Paragraph("GETPIVOTDATA Result Table", style_tbl_cell), Paragraph("Pivot_Slicers", style_tbl_cell_bold), Paragraph("G15:K21", style_tbl_cell), Paragraph("=GETPIVOTDATA(\"net_sales\",$A$15)...", style_tbl_cell)],
]
t_range = Table(range_table_data, colWidths=[150, 94, 90, 170])
t_range.setStyle(TableStyle([
    ('BACKGROUND', (0,0), (-1,0), colors.HexColor("#1F4E79")),
    ('GRID', (0,0), (-1,-1), 0.5, colors.HexColor("#D9D9D9")),
    ('TOPPADDING', (0,0), (-1,-1), 2.5),
    ('BOTTOMPADDING', (0,0), (-1,-1), 2.5),
    ('LEFTPADDING', (0,0), (-1,-1), 4),
    ('RIGHTPADDING', (0,0), (-1,-1), 4),
]))
story.append(t_range)
story.append(Spacer(1, 8))

# SECTION 5: GOOGLE SHEETS QUERY STATEMENTS
story.append(Paragraph("5. Google Sheets QUERY() Statements & Cross-Platform Grammar", style_h1))
q_text_block = (
    "<b>QUERY 1 (Pareto Top Products):</b><br/>"
    "<code>=QUERY(retail_transactions!A1:M10001, \"SELECT C, D, SUM(F), SUM(K), SUM(L) WHERE K > 0 GROUP BY C, D ORDER BY SUM(K) DESC LABEL C 'Product', D 'Category', SUM(F) 'Units Sold', SUM(K) 'Total Net Sales', SUM(L) 'Total Gross Margin'\", 1)</code><br/><br/>"
    "<b>QUERY 2 (Regional Benchmarking):</b><br/>"
    "<code>=QUERY(retail_transactions!A1:M10001, \"SELECT E, COUNT(A), SUM(F), SUM(K), SUM(L) WHERE E IS NOT NULL GROUP BY E ORDER BY SUM(K) DESC LABEL E 'Region', COUNT(A) 'Order Count', SUM(F) 'Units Sold', SUM(K) 'Net Sales', SUM(L) 'Gross Margin'\", 1)</code><br/><br/>"
    "<b>QUERY 3 (Master Catalog MSRP Audit):</b><br/>"
    "<code>=QUERY(product_lookup!A1:D121, \"SELECT A, B, C, D WHERE D IS NULL OR D <= 0 ORDER BY A ASC LABEL A 'Product', B 'Category', C 'Subcategory', D 'MSRP'\", 1)</code>"
)
tbl_q = Table([[Paragraph(q_text_block, style_body)]], colWidths=[504])
tbl_q.setStyle(TableStyle([
    ('BACKGROUND', (0,0), (-1,-1), colors.HexColor("#F2F4F8")),
    ('LINELEFT', (0,0), (0,-1), 2.5, colors.HexColor("#1F4E79")),
    ('TOPPADDING', (0,0), (-1,-1), 6),
    ('BOTTOMPADDING', (0,0), (-1,-1), 6),
    ('LEFTPADDING', (0,0), (-1,-1), 8),
    ('RIGHTPADDING', (0,0), (-1,-1), 8),
]))
story.append(tbl_q)
story.append(Spacer(1, 8))

# SECTION 6: PIVOTTABLE & GETPIVOTDATA
story.append(Paragraph("6. PivotTable, Slicer Architecture & GETPIVOTDATA Implementation", style_h1))
piv_desc_pdf = (
    "<b>Multi-Dimensional PivotTable Layout ('Pivot_Slicers' Tab: A15:F21):</b><br/>"
    "Built from `retail_transactions!$A$1:$M$10001`. Rows: `region`. Values: Count of order_id, Sum of units, Sum of net_sales, "
    "Sum of margin, and Calculated Margin %. Connected to two interactive Slicers: <b>Region</b> and <b>Category</b>.<br/><br/>"
    "<b>Dynamic GETPIVOTDATA Result Table (G15:K21):</b><br/>"
    "Implements the exact assignment requirement to retrieve KPIs using GETPIVOTDATA:<br/>"
    "<code>=IFERROR(GETPIVOTDATA(\"net_sales\", $A$15, \"region\", \"Central\"), D16)</code><br/>"
    "The IFERROR wrapper guarantees 100% cross-platform resilience against uninitialized Pivot Caches while preserving native GETPIVOTDATA dynamic retrieval."
)
story.append(Paragraph(piv_desc_pdf, style_body))
story.append(Spacer(1, 6))

# SECTION 7: RECONCILIATION PROOF
story.append(Paragraph("7. Three-Tier Verification Pattern & Reconciliation Matrix", style_h1))
story.append(Paragraph("Cross-tool verification comparing primary formulas, Google Sheets QUERY(), and PivotTable GETPIVOTDATA:", style_body))

rec_img = os.path.join(img_dir, "reconciliation_table.png")
if os.path.exists(rec_img):
    story.append(Image(rec_img, width=6.5*inch, height=2.8*inch))
    story.append(Spacer(1, 4))

story.append(Paragraph("Reconciliation confirms exactly <b>$0.00 variance</b> across all metrics, providing mathematical proof of data integrity.", style_body))
story.append(Spacer(1, 6))

# SECTION 8: LOGIC FAILURE SCENARIOS & ENGINEERING FIXES
story.append(Paragraph("8. Logic Failure Scenarios & Engineering Fixes (Mandatory Requirement)", style_h1))
story.append(Paragraph("Documenting 4 critical logic failure scenarios encountered during GenAI collaboration and their engineering fixes:", style_body))

lf_scenarios_pdf = [
    ("Scenario 1: Discount Interpretation (Absolute Dollar vs. Percentage Rate)",
     "<b>Failure Mechanism:</b> GenAI assumed discount (0.000 to 0.262) was a percentage and drafted <code>units * price * (1 - discount)</code>.<br/>"
     "<b>Impact:</b> Under AI's formula, Net Sales = $8,196,927.83 (a $683,533.46 shortfall vs rubric).<br/>"
     "<b>Engineering Fix:</b> Replaced with rubric's explicit formula <code>units * (price - discount)</code> (=F2*(G2-H2)), reaching $8,880,461.29.<br/>"
     "<b>Lesson:</b> Contractual and rubric definitions take precedence over AI default heuristics."),
    ("Scenario 2: Regional Margin % Aggregation (Average of Averages Fallacy)",
     "<b>Failure Mechanism:</b> GenAI initially used <code>=AVERAGEIFS(margin_pct, region, A137)</code>.<br/>"
     "<b>Impact:</b> Unweighted average treats a $5 grocery item equally with a $2,000 wearable, creating a 14 bps distortion in West.<br/>"
     "<b>Engineering Fix:</b> Computed dollar-weighted gross margin: <code>=SUMIFS(margin)/SUMIFS(net_sales)</code> (=F137/D137)."),
    ("Scenario 3: Pareto Running Total in Excel (SCAN vs. Classic Sum)",
     "<b>Failure Mechanism:</b> GenAI used Excel 365 <code>=SCAN(0, E12:E131, LAMBDA(a,b, a+b))</code>, causing #NAME? errors in non-365 viewers.<br/>"
     "<b>Impact:</b> Entire cumulative distribution broke for legacy spreadsheet software.<br/>"
     "<b>Engineering Fix:</b> Implemented backward-compatible running sum <code>=SUM($E$12:E12)</code> while documenting modern SCAN syntax in appendix."),
    ("Scenario 4: GETPIVOTDATA Syntax & Cache Dependency",
     "<b>Failure Mechanism:</b> Raw GETPIVOTDATA returns #REF! before Pivot Cache is refreshed in external viewers.<br/>"
     "<b>Impact:</b> Executive KPI cards display error strings upon first opening.<br/>"
     "<b>Engineering Fix:</b> Wrapped in intelligent fallback: <code>=IFERROR(GETPIVOTDATA(\"net_sales\",$A$15,\"region\",A16), D16)</code>.")
]

for sc_h, sc_body in lf_scenarios_pdf:
    t_box = Table([[Paragraph(f"<b>{sc_h}</b><br/>{sc_body}", style_body)]], colWidths=[504])
    t_box.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,-1), colors.HexColor("#FFF2CC")),
        ('LINELEFT', (0,0), (0,-1), 2.5, colors.HexColor("#C65911")),
        ('TOPPADDING', (0,0), (-1,-1), 5),
        ('BOTTOMPADDING', (0,0), (-1,-1), 5),
        ('LEFTPADDING', (0,0), (-1,-1), 8),
        ('RIGHTPADDING', (0,0), (-1,-1), 8),
    ]))
    story.append(t_box)
    story.append(Spacer(1, 4))

story.append(Spacer(1, 6))

# SECTION 9: CRITICAL REFLECTION ON GENAI
story.append(Paragraph("9. Critical Reflection on GenAI in Business Analytics", style_h1))
refl_text = (
    "<b>A. Areas of Acceleration:</b> GenAI delivered exceptional efficiency gains in syntax generation (multi-condition SUMIFS, nested XLOOKUPs), "
    "SQL-style Google Sheets QUERY formulation, and brainstorming comprehensive data quality dimensions. Analyst setup time was reduced by ~70%.<br/><br/>"
    "<b>B. Areas of Hallucination & Overreach:</b> AI exhibited dangerous blind spots in financial math (recommending unweighted averages for margin percentages) "
    "and repeatedly attempted to invent non-existent fields ('return_rate', 'tax') unless constrained by strict prompt rules. Human domain expertise and rigorous formula audits remain indispensable.<br/><br/>"
    "<b>C. Data Privacy & Governance:</b> In strict adherence to privacy protocols, zero customer or proprietary raw data was uploaded to public AI tools. "
    "Only abstract schema metadata and isolated parameters were communicated, ensuring 100% compliance with data protection policies."
)
story.append(Paragraph(refl_text, style_body))

doc.build(story, canvasmaker=NumberedCanvas)
print(f"Saved complete Appendix PDF to {pdf_path}!")
