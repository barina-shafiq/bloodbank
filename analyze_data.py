import pandas as pd
import numpy as np

df_trans = pd.read_csv(r'C:/Users/Barina/.gemini/antigravity/brain/396e2150-5a1e-45a9-86bd-fa5166779ee1/.user_uploaded/media_1792076299225_5129d5ee.csv')
df_prod = pd.read_csv(r'C:/Users/Barina/.gemini/antigravity/brain/396e2150-5a1e-45a9-86bd-fa5166779ee1/.user_uploaded/media_1792076303653_816cf369.csv')

df_trans['net_sales'] = df_trans['units'] * (df_trans['price'] - df_trans['discount'])
df_trans['cogs'] = df_trans['units'] * df_trans['cost']
df_trans['margin'] = df_trans['net_sales'] - df_trans['cogs']

prod_agg = df_trans.groupby('product').agg(
    category=('category', 'first'),
    total_units=('units', 'sum'),
    total_sales=('net_sales', 'sum'),
    total_margin=('margin', 'sum')
).reset_index()

prod_agg = prod_agg.sort_values(by='total_sales', ascending=False).reset_index(drop=True)
prod_agg['rank'] = prod_agg.index + 1
prod_agg['sales_share'] = prod_agg['total_sales'] / prod_agg['total_sales'].sum()
prod_agg['cum_sales'] = prod_agg['total_sales'].cumsum()
prod_agg['cum_sales_pct'] = prod_agg['cum_sales'] / prod_agg['total_sales'].sum()
prod_agg['cum_product_pct'] = prod_agg['rank'] / len(prod_agg)

print('=== PARETO SUMMARY ===')
print('Total Products:', len(prod_agg))
top_24_share = prod_agg.loc[23, 'cum_sales_pct']
print('Top 24 products (exact 20%): cum revenue share =', round(top_24_share * 100, 2), '%')

cutoff_80 = (prod_agg['cum_sales_pct'] >= 0.80).idxmax()
print('Product reaching >= 80%:', cutoff_80 + 1, 'products with', round(prod_agg.loc[cutoff_80, 'cum_sales_pct'] * 100, 2), '%')

print('\nTop 25 Products by Revenue:')
for idx, r in prod_agg.head(26).iterrows():
    print(f"{int(r['rank'])}. {r['product']} ({r['category']}): Sales=${r['total_sales']:,.2f} Share={r['sales_share']*100:.2f}% CumSales=${r['cum_sales']:,.2f} CumShare={r['cum_sales_pct']*100:.2f}%")

print('\n=== REGIONAL SUMMARY ===')
reg_agg = df_trans.groupby('region').agg(
    orders=('order_id', 'count'),
    units=('units', 'sum'),
    net_sales=('net_sales', 'sum'),
    cogs=('cogs', 'sum'),
    margin=('margin', 'sum')
).reset_index()
reg_agg['margin_pct'] = reg_agg['margin'] / reg_agg['net_sales']
total_sales = reg_agg['net_sales'].sum()
total_margin = reg_agg['margin'].sum()
print(f'Company Total: Sales=${total_sales:,.2f}, Margin=${total_margin:,.2f}, Margin%={total_margin/total_sales*100:.2f}%')
reg_agg = reg_agg.sort_values(by='margin_pct', ascending=True).reset_index(drop=True)
for idx, r in reg_agg.iterrows():
    print(f"{r['region']}: Orders={r['orders']}, Units={r['units']}, Sales=${r['net_sales']:,.2f}, Margin=${r['margin']:,.2f}, Margin%={r['margin_pct']*100:.2f}%")

# Check threshold
target_thresh = 0.285  # 28.5%
print(f'\nRegions below target margin threshold ({target_thresh*100:.1f}%):')
for idx, r in reg_agg.iterrows():
    status = 'BELOW TARGET' if r['margin_pct'] < target_thresh else 'MEETS TARGET'
    print(f"  {r['region']}: {r['margin_pct']*100:.2f}% -> {status} (Diff: {(r['margin_pct'] - target_thresh)*100:+.2f}%)")
