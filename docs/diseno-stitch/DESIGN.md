---
name: Event Ticketing & Operations System
colors:
  surface: '#f8f9ff'
  surface-dim: '#cbdbf5'
  surface-bright: '#f8f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#eff4ff'
  surface-container: '#e5eeff'
  surface-container-high: '#dce9ff'
  surface-container-highest: '#d3e4fe'
  on-surface: '#0b1c30'
  on-surface-variant: '#45464d'
  inverse-surface: '#213145'
  inverse-on-surface: '#eaf1ff'
  outline: '#76777d'
  outline-variant: '#c6c6cd'
  surface-tint: '#565e74'
  primary: '#000000'
  on-primary: '#ffffff'
  primary-container: '#131b2e'
  on-primary-container: '#7c839b'
  inverse-primary: '#bec6e0'
  secondary: '#0051d5'
  on-secondary: '#ffffff'
  secondary-container: '#316bf3'
  on-secondary-container: '#fefcff'
  tertiary: '#000000'
  on-tertiary: '#ffffff'
  tertiary-container: '#001e2c'
  on-tertiary-container: '#008ebf'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dae2fd'
  primary-fixed-dim: '#bec6e0'
  on-primary-fixed: '#131b2e'
  on-primary-fixed-variant: '#3f465c'
  secondary-fixed: '#dbe1ff'
  secondary-fixed-dim: '#b4c5ff'
  on-secondary-fixed: '#00174b'
  on-secondary-fixed-variant: '#003ea8'
  tertiary-fixed: '#c4e7ff'
  tertiary-fixed-dim: '#7bd0ff'
  on-tertiary-fixed: '#001e2c'
  on-tertiary-fixed-variant: '#004c69'
  background: '#f8f9ff'
  on-background: '#0b1c30'
  surface-variant: '#d3e4fe'
typography:
  display-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 3rem
    fontWeight: '700'
    lineHeight: 3.5rem
    letterSpacing: -0.025em
  display-lg-mobile:
    fontFamily: Plus Jakarta Sans
    fontSize: 2rem
    fontWeight: '700'
    lineHeight: 2.5rem
    letterSpacing: -0.02em
  headline-xl:
    fontFamily: Plus Jakarta Sans
    fontSize: 2.25rem
    fontWeight: '700'
    lineHeight: 2.75rem
    letterSpacing: -0.02em
  headline-xl-mobile:
    fontFamily: Plus Jakarta Sans
    fontSize: 1.75rem
    fontWeight: '700'
    lineHeight: 2.25rem
    letterSpacing: -0.015em
  headline-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 1.5rem
    fontWeight: '600'
    lineHeight: 2rem
    letterSpacing: -0.015em
  headline-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 1.25rem
    fontWeight: '600'
    lineHeight: 1.75rem
    letterSpacing: -0.01em
  title-sm:
    fontFamily: Plus Jakarta Sans
    fontSize: 1.125rem
    fontWeight: '600'
    lineHeight: 1.5rem
    letterSpacing: -0.005em
  body-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 1rem
    fontWeight: '400'
    lineHeight: 1.5rem
    letterSpacing: 0em
  body-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 0.875rem
    fontWeight: '400'
    lineHeight: 1.25rem
    letterSpacing: 0em
  body-sm:
    fontFamily: Plus Jakarta Sans
    fontSize: 0.75rem
    fontWeight: '400'
    lineHeight: 1rem
    letterSpacing: 0em
  label-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 0.875rem
    fontWeight: '500'
    lineHeight: 1.25rem
    letterSpacing: 0.01em
  label-sm:
    fontFamily: Plus Jakarta Sans
    fontSize: 0.75rem
    fontWeight: '600'
    lineHeight: 1rem
    letterSpacing: 0.02em
  code-sm:
    fontFamily: JetBrains Mono
    fontSize: 0.75rem
    fontWeight: '500'
    lineHeight: 1rem
    letterSpacing: 0.05em
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  gutter: 1.5rem
  gutter-mobile: 1rem
  margin: 2rem
  margin-mobile: 1rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 1rem
  space-lg: 1.5rem
  space-xl: 2rem
  space-2xl: 3rem
---

## Brand & Style

This design system establishes a high-precision, utilitarian aesthetic tailored for dual-sided event ticketing platforms: high-volume B2C consumer purchasing and complex B2B event management operations.

### Personality & Emotional Response
The interface evokes absolute operational reliability, transactional security, and frictionless clarity. It eschews decorative fluff, skeuomorphic excess, and heavy atmospheric blurs in favor of structural authority, high text legibility, and predictable spatial pacing. Users should feel complete confidence during checkout, seat selection, and mission-critical administrative tasks (e.g., live attendee check-in, revenue tracking, and capacity gating).

### Visual Direction
- **Style Archetype:** Modern Corporate SaaS meets high-clarity transactional commerce.
- **Surface Hierarchy:** Pure white container layers lifted subtly over a clinical, ultra-light slate base canvas.
- **Visual Discipline:** Strict geometric precision, flat low-contrast keylines, deliberate spacing rhythms, and high-visibility status tokens. Visual decoration is restricted exclusively to structural iconography (monoline SVG) and discrete transactional artifacts (e.g., ticket perforation lines).

## Colors

The color system prioritizes structural hierarchy and functional status over expressive ornamentation.

### Foundation & Surfaces
- **Canvas Base:** `#F8FAFC` (Slate 50) serves as the full-viewport application background for both admin consoles and checkout flows.
- **Surface Level 1:** `#FFFFFF` (Pure White) defines content cards, data tables, modal dialogs, and persistent sidebars.
- **Surface Level 2 (Subtle inset):** `#F1F5F9` (Slate 100) used for table headers, segmented control backdrops, and disabled containers.
- **Borders & Dividers:** `#E2E8F0` (Slate 200) for standard layout and component keylines; `#CBD5E1` (Slate 300) for interactive control borders on hover.

### Brand Roles
- **Primary (`#0F172A` / `#1E293B`):** Deep slate/navy. Anchors primary button actions, main typography headers, navigation states, and dark status banners.
- **Secondary (`#2563EB` / Active: `#1D4ED8`):** Professional cobalt blue. Designated exclusively for primary interactive links, primary call-to-action fills, active focus rings, and selection indicators.
- **Tertiary / Accent (`#38BDF8` / `#60A5FA`):** Controlled sapphire tint. Used strictly for subtle focus accents, metric visual cues, and secondary data highlights. Never use for standard body copy or high-contrast UI controls.
- **Neutrals:**
  - High Emphasis Text: `#0F172A`
  - Medium Emphasis / Body Secondary: `#475569`
  - Subdued / Metadata: `#64748B`
  - Placeholder / Inactive: `#94A3B8`

### Semantic Feedback Tokens
Statuses follow a three-tier token formula (Text/Icon + Container Fill + Structural Stroke):
- **Success (Confirmed, Paid, Active):** 
  - Text/Icon: `#16A34A`
  - Background: `#DCFCE7`
  - Border: `#86EFAC`
- **Warning / Pending (Awaiting Payment, Reserved, Flagged):** 
  - Text/Icon: `#D97706`
  - Background: `#FEF3C7`
  - Border: `#FCD34D`
- **Critical / Error (Sold Out, Canceled, Payment Failed):** 
  - Text/Icon: `#DC2626`
  - Background: `#FEE2E2`
  - Border: `#FCA5A5`

## Typography

The typographic hierarchy uses **Plus Jakarta Sans** uniformly across headlines, titles, copy, and UI controls to deliver a geometric yet warm, highly legible presence across dense administrative dashboards and quick-scan mobile checkout views. **JetBrains Mono** is reserved exclusively for alphanumeric ticket references, barcodes, order IDs, and payment references.

### Application Rules
- **Numerical Data Alignment:** For table columns displaying ticket inventory, pricing, gross revenue, and seats, apply tabular figures (`font-variant-numeric: tabular-nums`) to ensure vertical alignment.
- **Micro-labels & Status Pills:** Use `label-sm` with explicit uppercase transformations (`text-transform: uppercase`) and slight positive letter-spacing (`0.02em` to `0.05em`) strictly within table status badges and metadata headers.
- **Form Labels:** Set form input labels using `label-md` in `#0F172A` with weight 500. Avoid thin or muted labels that reduce input usability.

## Layout & Spacing

The layout is constructed around an 8-point structural grid and a predictable 12-column layout for administrative and multi-step purchasing interfaces.

### Layout Grid Models
- **Administrative Portal:** Persistent left navigation sidebar fixed at `260px` width (collapsible to `72px` icon rail on viewports under 1280px). Content pane operates on a fluid 12-column grid with `gutter` of `1.5rem` (`24px`) and outer canvas `margin` of `2rem` (`32px`).
- **Consumer Event / Checkout Portal:** Fixed centered container capped at `1200px` max width for event browsing, and `840px` for multi-step checkout modals and ticket wizards.
- **Mobile Handheld (<768px):** Single-column stack, outer canvas `margin-mobile` of `1rem` (`16px`), `gutter-mobile` of `1rem` (`16px`). Bottom-sheet or persistent sticky bottom bar for checkout actions (`$total` and "Comprar Entradas").

### Spacing Rhythms
- **`space-xs` (4px):** Internal icon-to-label spacing, badge vertical padding.
- **`space-sm` (8px):** Form field gap to helper text, dense list item padding.
- **`space-md` (16px):** Form field padding, regular cell vertical padding, card inner padding (compact).
- **`space-lg` (24px):** Standard card padding, modal content padding, metric widget padding.
- **`space-xl` (32px):** Grid section separations, dashboard row gaps.
- **`space-2xl` (48px):** Page header to content block separation.

## Elevation & Depth

Visual separation relies on a **border-first, ambient-shadow** hierarchy. Surfaces avoid thick black shadows or deep blur cones, maintaining an architectural, enterprise-grade finish.

### Elevation Levels
- **Level 0 (Flat Canvas):** 
  - Background: `#F8FAFC`
  - Border: None
  - Shadow: None
- **Level 1 (Card & Inset Containers):**
  - Background: `#FFFFFF`
  - Border: `1px solid #E2E8F0`
  - Shadow: `0 1px 2px 0 rgba(15, 23, 42, 0.05)`
  - Use case: Standard dashboard cards, event inventory rows, static ticket containers.
- **Level 2 (Hover & Active Interactive Items):**
  - Background: `#FFFFFF`
  - Border: `1px solid #CBD5E1`
  - Shadow: `0 4px 6px -1px rgba(15, 23, 42, 0.07), 0 2px 4px -2px rgba(15, 23, 42, 0.05)`
  - Use case: Interactive event cards on hover, table row selection hover.
- **Level 3 (Dropdowns & Popovers):**
  - Background: `#FFFFFF`
  - Border: `1px solid #E2E8F0`
  - Shadow: `0 10px 15px -3px rgba(15, 23, 42, 0.08), 0 4px 6px -4px rgba(15, 23, 42, 0.04)`
  - Use case: Date pickers, attendee filter menus, profile dropdowns.
- **Level 4 (Modals & Checkout Sheets):**
  - Background: `#FFFFFF`
  - Border: `1px solid #E2E8F0`
  - Shadow: `0 20px 25px -5px rgba(15, 23, 42, 0.1), 0 8px 10px -6px rgba(15, 23, 42, 0.04)`
  - Overlay Backdrop: `#0F172A` at `40%` opacity (`rgba(15, 23, 42, 0.40)`) with `4px` backdrop blur.

## Shapes

The design system enforces a balanced roundedness formula (Option 2 - Rounded) that pairs functional discipline with modern consumer friendliness.

### Corner Radius Mapping
- **Base Components (`rounded`, `0.5rem` / `8px`):** Standard buttons, text inputs, select dropdowns, search bars, and ticket stub validation stamps.
- **Large Components (`rounded-lg`, `1rem` / `16px`):** Event preview cards, data panels, checkout invoice summaries, and digital tickets.
- **Modal & Sheet Corners (`rounded-xl`, `1.5rem` / `24px`):** Dialog windows, bottom checkout sheets (top corners only on mobile).
- **Pill Badges (`rounded-full`, `9999px`):** Status badges (Aprobado, Pendiente, Rechazado), counter indicators, and avatar wrappers.
- **Ticket Stubs:** Digital ticket containers feature inverted radial cutouts (`mask-image` or pseudo-element half-circles of `16px` diameter) placed along the vertical perforation divider.

## Components

### Buttons
- **Primary:** Background `#0F172A`, text `#FFFFFF`, border none, hover background `#1E293B`, focus ring `2px solid #2563EB` with `2px` offset.
- **Secondary (Action):** Background `#2563EB`, text `#FFFFFF`, hover background `#1D4ED8`.
- **Tertiary / Outline:** Background `#FFFFFF`, text `#0F172A`, border `1px solid #E2E8F0`, hover background `#F8FAFC`, hover border `#CBD5E1`.
- **Destructive:** Background `#DC2626`, text `#FFFFFF`, hover background `#B91C1C`.
- **Dimensions:**
  - Default: Height `40px`, horizontal padding `16px`, font `label-md`.
  - Large (Checkout CTA): Height `48px`, horizontal padding `24px`, font `headline-md` at 16px weight 600.
  - Small (Table Actions): Height `32px`, horizontal padding `12px`, font `label-sm`.

### Status Badges & Chips
- **Geometry:** Height `24px`, horizontal padding `8px`, border radius `9999px` (`rounded-full`), gap `4px` between icon and label.
- **Typographic Treatment:** `label-sm` (11px or 12px, font-weight 600).
- **Status Variants:**
  - *Pagado / Aprobado:* Background `#DCFCE7`, border `1px solid #86EFAC`, text `#16A34A`, leading SVG dot/check icon (12px).
  - *Pendiente:* Background `#FEF3C7`, border `1px solid #FCD34D`, text `#D97706`, leading SVG clock icon (12px).
  - *Cancelado / Rechazado:* Background `#FEE2E2`, border `1px solid #FCA5A5`, text `#DC2626`, leading SVG alert icon (12px).
  - *Neutro / Borrador:* Background `#F1F5F9`, border `1px solid #E2E8F0`, text `#475569`.

### Input Fields & Controls
- **Field Layout:** Vertical arrangement with label (`label-md`, `#0F172A`) sitting `6px` above the input. Subtext or error message sits `4px` below.
- **Input Box:** Height `40px`, background `#FFFFFF`, border `1px solid #E2E8F0`, radius `8px`, padding `0 12px`.
- **States:**
  - *Focus:* Border `#2563EB`, box-shadow `0 0 0 3px rgba(37, 99, 235, 0.15)`.
  - *Error:* Border `#DC2626`, box-shadow `0 0 0 3px rgba(220, 38, 38, 0.15)`.
- **Checkboxes & Radios:** `16px × 16px`, border `1.5px solid #CBD5E1`, radius `4px` (checkbox) / `50%` (radio). Checked state fills `#2563EB` with white interior icon.

### SaaS Data Tables (Administrative)
- **Table Container:** Pure white background, border `1px solid #E2E8F0`, radius `12px`, overflow hidden.
- **Header:** Height `44px`, background `#F8FAFC`, border bottom `1px solid #E2E8F0`, text `label-sm` in `#64748B`, uppercase tracking.
- **Rows:** Height `56px`, border bottom `1px solid #F1F5F9`, hover background `#F8FAFC`. Alternating zebra stripes are forbidden.
- **Cells:** Vertical alignment centered, numerical columns right-aligned with `JetBrains Mono` or tabular numerals.

### Event Cards (B2C Showcase)
- **Structure:** White card, radius `16px`, border `1px solid #E2E8F0`, shadow level 1.
- **Image Header:** Aspect ratio 16:9, rounded top `15px`, contains floating absolute category badge top-left.
- **Body Content:** Date and time in `label-sm` secondary color (`#2563EB`), title in `headline-md` (`#0F172A`), location line with monoline map-pin SVG in `#64748B`.
- **Footer:** Separated by `1px solid #F1F5F9`. Left: price formatted as "Desde $XX.XX" in `title-sm`. Right: "Comprar" action button.

### Digital Tickets (Voucher / Wallet Component)
- **Anatomy:** Composed of two coupled containers (Main Ticket Body + Tear-off Stub).
- **Perforation Line:** Dashed border `1px dashed #CBD5E1` flanked on top and bottom by symmetrical semi-circular cutouts (radius `8px`, filled with canvas background `#F8FAFC`).
- **Body:** Event name, venue tier, seat/zone numbers, gate assignment, attendee name.
- **Stub:** High-contrast QR/Aztec code (`120px × 120px`), verification code in `code-sm` (`JetBrains Mono`), validation status badge.