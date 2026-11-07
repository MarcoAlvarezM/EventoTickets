// Tokens de diseño de Stitch ("Event Ticketing & Operations System" / DESIGN.md).
// Se cargan después del script de Tailwind (CDN) igual que en las pantallas originales.
tailwind.config = {
  darkMode: "class",
  theme: {
    extend: {
      colors: {
        "inverse-primary": "#bec6e0", "on-primary-fixed": "#131b2e", "error": "#ba1a1a", "surface-container-high": "#dce9ff",
        "on-background": "#0b1c30", "on-tertiary-fixed-variant": "#004c69", "surface-container": "#e5eeff", "tertiary-fixed": "#c4e7ff",
        "surface-container-lowest": "#ffffff", "on-tertiary": "#ffffff", "surface-tint": "#565e74", "on-secondary": "#ffffff",
        "tertiary-container": "#001e2c", "on-error-container": "#93000a", "surface-variant": "#d3e4fe", "on-tertiary-fixed": "#001e2c",
        "surface": "#f8f9ff", "error-container": "#ffdad6", "on-primary": "#ffffff", "secondary-fixed-dim": "#b4c5ff",
        "primary-container": "#131b2e", "secondary-container": "#316bf3", "tertiary-fixed-dim": "#7bd0ff", "secondary-fixed": "#dbe1ff",
        "inverse-on-surface": "#eaf1ff", "surface-container-low": "#eff4ff", "on-error": "#ffffff", "surface-bright": "#f8f9ff",
        "surface-dim": "#cbdbf5", "surface-container-highest": "#d3e4fe", "outline-variant": "#c6c6cd", "on-secondary-fixed": "#00174b",
        "outline": "#76777d", "on-secondary-fixed-variant": "#003ea8", "on-surface": "#0b1c30", "on-primary-container": "#7c839b",
        "tertiary": "#000000", "on-surface-variant": "#45464d", "on-secondary-container": "#fefcff", "on-primary-fixed-variant": "#3f465c",
        "primary": "#000000", "secondary": "#0051d5", "on-tertiary-container": "#008ebf", "inverse-surface": "#213145",
        "background": "#f8f9ff", "primary-fixed-dim": "#bec6e0", "primary-fixed": "#dae2fd"
      },
      borderRadius: { DEFAULT: "0.25rem", lg: "0.5rem", xl: "0.75rem", full: "9999px" },
      spacing: {
        "space-sm": "0.5rem", "space-2xl": "3rem", "space-xl": "2rem", "space-xs": "0.25rem", "gutter": "1.5rem",
        "gutter-mobile": "1rem", "space-md": "1rem", "space-lg": "1.5rem", "margin-mobile": "1rem", "margin": "2rem"
      },
      fontFamily: {
        "display-lg-mobile": ["Plus Jakarta Sans"], "label-md": ["Plus Jakarta Sans"], "headline-lg": ["Plus Jakarta Sans"],
        "headline-xl": ["Plus Jakarta Sans"], "body-md": ["Plus Jakarta Sans"], "code-sm": ["JetBrains Mono"], "title-sm": ["Plus Jakarta Sans"],
        "body-lg": ["Plus Jakarta Sans"], "label-sm": ["Plus Jakarta Sans"], "body-sm": ["Plus Jakarta Sans"],
        "headline-xl-mobile": ["Plus Jakarta Sans"], "display-lg": ["Plus Jakarta Sans"], "headline-md": ["Plus Jakarta Sans"]
      },
      fontSize: {
        "display-lg-mobile": ["2rem", { lineHeight: "2.5rem", letterSpacing: "-0.02em", fontWeight: "700" }],
        "label-md": ["0.875rem", { lineHeight: "1.25rem", letterSpacing: "0.01em", fontWeight: "500" }],
        "headline-lg": ["1.5rem", { lineHeight: "2rem", letterSpacing: "-0.015em", fontWeight: "600" }],
        "headline-xl": ["2.25rem", { lineHeight: "2.75rem", letterSpacing: "-0.02em", fontWeight: "700" }],
        "body-md": ["0.875rem", { lineHeight: "1.25rem", letterSpacing: "0em", fontWeight: "400" }],
        "code-sm": ["0.75rem", { lineHeight: "1rem", letterSpacing: "0.05em", fontWeight: "500" }],
        "title-sm": ["1.125rem", { lineHeight: "1.5rem", letterSpacing: "-0.005em", fontWeight: "600" }],
        "body-lg": ["1rem", { lineHeight: "1.5rem", letterSpacing: "0em", fontWeight: "400" }],
        "label-sm": ["0.75rem", { lineHeight: "1rem", letterSpacing: "0.02em", fontWeight: "600" }],
        "body-sm": ["0.75rem", { lineHeight: "1rem", letterSpacing: "0em", fontWeight: "400" }],
        "headline-xl-mobile": ["1.75rem", { lineHeight: "2.25rem", letterSpacing: "-0.015em", fontWeight: "700" }],
        "display-lg": ["3rem", { lineHeight: "3.5rem", letterSpacing: "-0.025em", fontWeight: "700" }],
        "headline-md": ["1.25rem", { lineHeight: "1.75rem", letterSpacing: "-0.01em", fontWeight: "600" }]
      }
    }
  }
};
