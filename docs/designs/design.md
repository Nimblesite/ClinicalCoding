# Design System Specification: Clinical Precision & Tonal Depth
 
## 1. Overview & Creative North Star
### The Creative North Star: "The Clinical Curator"
In the high-stakes world of clinical coding and healthcare data, trust is not built with loud colors or rigid grids—it is built through **Clarity, Calm, and Precision.** This design system moves away from the "standard dashboard" template by adopting an editorial approach to data. We treat the UI as a curated environment where information is layered naturally, rather than boxed in.
 
The system utilizes **Intentional Asymmetry** and **Tonal Depth** to guide the eye. By breaking the traditional 1px border habit and moving toward a "physically layered" philosophy, we create a workspace that feels sophisticated, authoritative, and breathable.
 
---
 
## 2. Colors & Surface Philosophy
The palette is rooted in a deep, professional blue and a clean, sterile white, augmented by a range of neutral "surface" tiers that define the architecture of the interface.
 
### The "No-Line" Rule
**Explicit Instruction:** Use of 1px solid borders for sectioning or containment is strictly prohibited. 
- Boundaries must be defined through **background color shifts**. 
- Use `surface-container-low` for large section backgrounds and `surface-container-lowest` (Pure White) for foreground cards. This provides a crisp, high-trust distinction without visual "noise."
 
### Surface Hierarchy & Nesting
Treat the UI as a series of stacked, physical layers:
1.  **Base Layer:** `surface` (#f7f9fb) – The canvas of the application.
2.  **Section Layer:** `surface-container-low` (#f2f4f6) – For grouping related widgets or sidebar navigation.
3.  **Active Card Layer:** `surface-container-lowest` (#ffffff) – Used for primary data cards and interactive modules to provide the highest contrast.
 
### The "Glass & Gradient" Rule
To inject "soul" into the clinical environment:
- **Glassmorphism:** For floating elements like tooltips or pop-over menus, use `surface-container-lowest` at 80% opacity with a `20px` backdrop-blur.
- **Signature Gradients:** Main CTAs and Hero headers should utilize a subtle linear gradient: `primary` (#003fab) to `primary-container` (#0354dd) at a 135-degree angle. This adds a "premium" depth that flat hex codes cannot achieve.
 
---
 
## 3. Typography: The Editorial Voice
We use **Inter** for its mathematical precision and high legibility in dense data environments.
 
- **Display Scale (`display-lg` to `display-sm`):** Reserved for high-level data summaries (e.g., total patient counts). Use a "tight" letter-spacing (-0.02em) to create an authoritative, editorial feel.
- **Headline & Title Scale:** Used for page headers and card titles. These provide the structural landmarks of the dashboard.
- **Body Scale:** Use `body-md` (0.875rem) as the workhorse for clinical notes and data entries to maintain a high information density without sacrificing comfort.
- **Label Scale:** Use `label-md` in all-caps with +0.05em tracking for secondary metadata or table headers to distinguish them clearly from actionable data.
 
---
 
## 4. Elevation & Depth
Hierarchy is achieved through **Tonal Layering** rather than structural lines.
 
- **The Layering Principle:** Depth is created by "stacking" tones. A `surface-container-lowest` card placed on a `surface-container-low` background creates a natural lift.
- **Ambient Shadows:** When a card requires a "floating" state (e.g., on hover), use an extra-diffused shadow:
  - `box-shadow: 0 12px 32px rgba(25, 28, 30, 0.04);` 
  - Never use pure black shadows. Always use a tinted version of `on-surface`.
- **The "Ghost Border" Fallback:** If a container requires further definition (e.g., in high-density data tables), use a "Ghost Border": `outline-variant` (#c2c6d4) at **15% opacity**.
 
---
 
## 5. Components
 
### Buttons
- **Primary:** Gradient-filled (`primary` to `primary-container`) with `md` (1.5rem) corner radius. Use `on-primary` text.
- **Secondary:** `surface-container-high` background with `on-secondary-container` text. No border.
- **Tertiary:** Transparent background, `primary` text. Use only for low-emphasis actions.
 
### Cards & Data Modules
- **Corner Radius:** All primary cards must use the `md` (1.5rem) or `lg` (2rem) scale.
- **Spacing:** Enforce a strict vertical rhythm using 24px or 32px gaps. **Forbid the use of divider lines.** Use white space to separate groups of information.
 
### Clinical Data Visualization
- **Status Indicators:** Use `tertiary` (#00524b) for "Success/Healthy," `error` (#ba1a1a) for "Critical," and `secondary` (#505f76) for "Neutral/Pending."
- **Charts:** Use a 4px stroke width for line charts. Use `primary` and `tertiary` as the primary data colors to maintain a high-trust, healthcare-appropriate aesthetic.
 
### Input Fields
- **Styling:** Use `surface-container-highest` for the input background. 
- **States:** On focus, transition the background to `surface-container-lowest` and apply a 2px "Ghost Border" using the `primary` color at 30% opacity.
 
---
 
## 6. Do's and Don'ts
 
### Do:
- **Do** use large typography for primary KPIs to create a clear visual entry point.
- **Do** utilize `xl` (3rem) rounding for decorative elements or pill-shaped status chips to soften the clinical feel.
- **Do** allow content to "breathe" with generous padding (min 24px) inside all containers.
 
### Don't:
- **Don't** use 1px solid borders to separate sidebar navigation from the main content; use a background shift to `surface-container-low`.
- **Don't** use pure black (#000000) for text. Always use `on-surface` (#191c1e) to reduce eye strain during long clinical shifts.
- **Don't** use "Default" drop shadows. If it doesn't look like ambient light, it's too heavy.