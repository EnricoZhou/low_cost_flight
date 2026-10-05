import { Injectable, signal, effect } from '@angular/core';

export type ThemeMode = 'dark' | 'light';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private readonly storageKey = 'skydeal-theme';
  readonly theme = signal<ThemeMode>('dark');

  constructor() {
    this.initTheme();

    // Effect to update DOM and storage whenever theme changes
    effect(() => {
      const currentTheme = this.theme();
      this.applyTheme(currentTheme);
    });
  }

  toggleTheme(): void {
    const nextTheme = this.theme() === 'dark' ? 'light' : 'dark';
    this.theme.set(nextTheme);
  }

  setTheme(mode: ThemeMode): void {
    this.theme.set(mode);
  }

  private initTheme(): void {
    if (typeof window === 'undefined') return;

    const saved = localStorage.getItem(this.storageKey) as ThemeMode | null;
    if (saved === 'dark' || saved === 'light') {
      this.theme.set(saved);
      return;
    }

    // Default to dark mode (matches mikes.cv initial aesthetic)
    this.theme.set('dark');
  }

  private applyTheme(mode: ThemeMode): void {
    if (typeof document === 'undefined') return;

    const root = document.documentElement;
    if (mode === 'dark') {
      root.classList.add('dark');
      root.classList.remove('light');
      root.setAttribute('data-theme', 'dark');
    } else {
      root.classList.add('light');
      root.classList.remove('dark');
      root.setAttribute('data-theme', 'light');
    }

    if (typeof window !== 'undefined') {
      localStorage.setItem(this.storageKey, mode);
    }
  }
}
