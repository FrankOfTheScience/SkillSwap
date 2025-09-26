# PR Template Usage Guide

This repository provides specialized pull request templates based on the type of change you're making.

## How to Use Specific Templates

### Method 1: URL Parameter (Recommended)
Add the template parameter to your PR creation URL:

```
# For patch changes (bug fixes, maintenance)
https://github.com/FrankOfTheScience/SkillSwap/compare/development...YOUR_BRANCH?template=patch.md

# For minor changes (new features)  
https://github.com/FrankOfTheScience/SkillSwap/compare/development...YOUR_BRANCH?template=minor.md

# For major changes (breaking changes)
https://github.com/FrankOfTheScience/SkillSwap/compare/development...YOUR_BRANCH?template=major.md
```

### Method 2: GitHub CLI
```bash
# Create PR with specific template
gh pr create --template .github/PULL_REQUEST_TEMPLATE/patch.md
gh pr create --template .github/PULL_REQUEST_TEMPLATE/minor.md  
gh pr create --template .github/PULL_REQUEST_TEMPLATE/major.md
```

### Method 3: Manual Selection
1. Create PR normally
2. Clear the description
3. Copy content from the appropriate template file
4. Fill in the sections

## Template Types

### 🐛 Patch Template (`patch.md`)
**Use for:**
- Bug fixes
- Documentation updates
- Code refactoring without behavior changes
- Dependency updates
- Security patches
- Performance optimizations

**Key Sections:**
- Root cause analysis
- Risk assessment
- Backward compatibility check
- Rollback plan

### ✨ Minor Template (`minor.md`)  
**Use for:**
- New features
- New API endpoints
- Enhanced functionality
- New integrations
- Performance improvements

**Key Sections:**
- Feature overview and user value
- Technical implementation details
- Testing strategy
- Performance considerations
- Security review

### 💥 Major Template (`major.md`)
**Use for:**
- Breaking changes
- API version changes
- Database schema changes
- Architecture changes
- Configuration breaking changes

**Key Sections:**
- Detailed breaking changes documentation
- Migration guide for users
- Risk assessment and mitigation
- Communication timeline
- Rollback strategy

## Best Practices

1. **Choose the Right Template:** Select based on semantic versioning impact
2. **Complete All Sections:** Don't skip sections - mark as N/A if not applicable
3. **Add Appropriate Labels:** Remember to add `patch`, `minor`, or `major` labels
4. **Link Issues:** Use "Fixes #123" to auto-close related issues
5. **Include Screenshots:** Visual changes should have before/after images
6. **Test Thoroughly:** Follow the testing checklists in each template

## Automatic Processing

When you label your PR with `patch`, `minor`, or `major`, the CI/CD pipeline will:
- Calculate the correct version bump
- Generate appropriate release notes
- Update the CHANGELOG
- Create a GitHub release

## Support

If you need help selecting the right template or have questions about the process, reach out to the maintainers or create an issue.