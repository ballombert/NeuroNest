# Architecture Decision Records (ADR)

Ce dossier contient les **Architecture Decision Records** du projet ADHDWorkspace.

## Qu'est-ce qu'un ADR?

Un ADR documente une décision architecturale importante avec:
- Le **contexte** et les contraintes
- La **décision** prise et sa justification
- Les **conséquences** (positives et négatives)
- Les **alternatives** considérées et rejetées

## Liste des ADRs

| # | Titre | Date | Status |
|---|-------|------|--------|
| [0001](0001-migration-maui-consolidation.md) | Migration vers MAUI et Consolidation Architecture | 2025-11-21 | Accepté |

## Convention de nommage

Format: `XXXX-description-courte.md`
- **XXXX**: Numéro séquentiel (0001, 0002, etc.)
- **description-courte**: Slug kebab-case descriptif

## Workflow

1. **Proposer** un ADR:
   ```bash
   # Copier le template
   cp template.md 0002-nouvelle-decision.md
   # Editer et mettre Status: Proposé
   ```

2. **Discuter** avec l'équipe (PR, réunion, chat)

3. **Accepter/Rejeter**:
   - Si accepté: Status → "Accepté", implémenter
   - Si rejeté: Status → "Rejeté", expliquer pourquoi dans Notes

4. **Déprécier** (si nécessaire):
   - Créer nouvel ADR qui remplace l'ancien
   - Mettre ancien Status → "Remplacé par ADR-YYYY"

## Principes

- [fa-solid fa-square-check] **Immutabilité**: Ne jamais supprimer un ADR accepté
- [fa-solid fa-square-check] **Concision**: 1-3 pages max, focus sur l'essentiel
- [fa-solid fa-square-check] **Contexte first**: Expliquer le "pourquoi" avant le "quoi"
- [fa-solid fa-square-check] **Trade-offs explicites**: Toujours lister conséquences négatives
- [fa-solid fa-square-check] **Alternatives documentées**: Montrer qu'on a considéré d'autres options

## Ressources

- [ADR GitHub Organization](https://adr.github.io/)
- [Thoughtworks Technology Radar on ADRs](https://www.thoughtworks.com/radar/techniques/lightweight-architecture-decision-records)
- [Template Markdown](template.md)

---

**Note**: Ce système ADR suit une approche minimaliste adaptée aux projets de taille moyenne. Pour des projets plus complexes, considérer des outils comme [adr-tools](https://github.com/npryce/adr-tools).
