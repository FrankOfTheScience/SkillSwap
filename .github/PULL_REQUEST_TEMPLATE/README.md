# PR Template Usage Guide

This repository provides specialized pull request templates with **automatic template application** based on the type of change you're making.

## 🤖 Automatic Template Application (Recommended)

The **easiest way** to use templates is through our automated system:

### How It Works
1. **Create a PR** from `development` to `staging` (or any target branch)
2. **Add a version label** (`major`, `minor`, or `patch`) to your PR
3. **✨ Magic happens automatically:**
   - PR title gets updated with proper prefix (`💥 BREAKING:`, `✨ feat:`, `🐛 fix:`)
   - PR description gets populated with the appropriate template
   - Bot adds helpful comment with next steps

### Smart Behavior
- **Won't overwrite** meaningful existing content
- **Asks permission** before replacing custom descriptions
- **Handles multiple labels** by using priority (major > minor > patch)
- **Provides helpful guidance** through automated comments

## 📝 Manual Template Methods (Alternative)

If you prefer manual control, you can still use these methods:

### Method 1: URL Parameter
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

### Method 3: Manual Copy-Paste
1. Create PR normally
2. Clear the description
3. Copy content from the appropriate template file
4. Fill in the sections

## Template Types

### 🐛 Patch Template (`patch.md`)
**Use for:**
- Bug fixes and hotfixes
- Documentation updates
- Code refactoring without behavior changes
- Dependency updates and security patches
- Performance optimizations
- Configuration fixes

**Key Sections:**
- Root cause analysis and solution details
- Risk assessment and testing verification
- Backward compatibility confirmation
- Rollback plan and deployment notes

### ✨ Minor Template (`minor.md`)  
**Use for:**
- New features and functionality
- New API endpoints and integrations
- Enhanced existing functionality
- Developer experience improvements
- Performance improvements
- New UI components or pages

**Key Sections:**
- Feature overview and user value proposition
- Technical implementation details
- Comprehensive testing strategy
- Performance and security considerations
- Documentation requirements

### 💥 Major Template (`major.md`)
**Use for:**
- Breaking changes and API modifications
- Database schema changes
- Architecture overhauls
- Configuration breaking changes
- Major version updates
- Incompatible dependency changes

**Key Sections:**
- Detailed breaking changes documentation
- Migration guide for users and developers
- Risk assessment and mitigation strategies
- Communication timeline and rollback plans
- Comprehensive testing and validation

## 🏷️ Version Labels

**Required:** Every PR to `staging` must have exactly one version label:

- **`patch`** - Bug fixes, maintenance (bumps 1.0.**1**)
- **`minor`** - New features, enhancements (bumps 1.**1**.0)  
- **`major`** - Breaking changes (bumps **2**.0.0)

The automation will:
- ✅ **Apply the correct template** automatically
- ✅ **Generate appropriate release notes**
- ✅ **Update the CHANGELOG** with your changes
- ✅ **Create GitHub releases** with proper versioning
- ✅ **Calculate semantic version** bumps automatically

## Best Practices

### For Contributors
1. **Always add a version label** - Required for PRs to staging
2. **Choose the right template** - Follow semantic versioning guidelines
3. **Complete all sections** - Don't skip sections, mark as N/A if not applicable
4. **Link related issues** - Use "Fixes #123", "Closes #456", "Related to #789"
5. **Include visual evidence** - Screenshots for UI changes, logs for bug fixes
6. **Test thoroughly** - Follow the testing checklists in each template

### For Reviewers
1. **Verify correct label** - Ensure the version impact matches the changes
2. **Check template completion** - All sections should be addressed
3. **Validate testing** - Ensure appropriate tests are included
4. **Review documentation** - Check if docs need updates
5. **Assess impact** - Verify risk assessment and rollback plans

## Automated Pipeline Integration

When you properly label your PR, the CI/CD pipeline will:

### On PR Creation/Update
- ✅ **Validate version label** is present
- ✅ **Apply appropriate template** automatically
- ✅ **Run comprehensive tests** (backend + frontend)
- ✅ **Check for common issues** and security concerns

### On PR Merge to Staging
- ✅ **Calculate semantic version** bump
- ✅ **Generate release notes** from your PR description
- ✅ **Update CHANGELOG.md** with structured entries
- ✅ **Create GitHub release** with proper tags
- ✅ **Trigger deployment** to staging environment

### Quality Gates
- 🔒 **Branch protection** - Direct pushes to main/staging require review
- 🧪 **Test requirements** - All tests must pass before merge
- 📋 **Label validation** - Version labels are mandatory
- 🔍 **Code review** - At least one approval required

## Troubleshooting

### Template Not Applied?
- **Check if label was added AFTER PR creation** - The automation only triggers on label addition
- **Remove and re-add the label** if it was added during PR creation
- **Check GitHub Actions** tab for any workflow errors
- **Verify you have the latest changes** - The automation was recently added

### Wrong Template Applied?
- **Edit the PR description** manually to correct it
- **Remove incorrect labels** and add the right one
- **The automation prioritizes**: major > minor > patch

### Multiple Labels?
- **Remove extra labels** - Only one version label should be present
- **The automation will warn** and use the highest priority label

## Git Hooks Integration

Our enhanced pre-push hooks provide additional quality checks:

- 📝 **Conventional commit** message validation
- 🧪 **Automated testing** before push
- 🔐 **Secret detection** to prevent credential leaks
- 🌿 **Branch protection** with helpful guidance
- 📊 **Code quality** checks (linting, type checking)

## Support

If you need help:
1. **Check the Actions tab** for workflow logs and errors
2. **Read template comments** for specific guidance
3. **Ask in PR comments** - Maintainers monitor all PRs
4. **Create an issue** for persistent problems

---

**🎉 This automated system makes contributing easier while maintaining high quality standards!**