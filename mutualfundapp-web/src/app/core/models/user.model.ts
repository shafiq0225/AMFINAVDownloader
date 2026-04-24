export type UserRole = 'Admin' | 'Employee' | 'User';
export type UserType = 'None' | 'HeadOfFamily' | 'FamilyMember';
export type ApprovalStatus = 'Pending' | 'Approved' | 'Rejected';

export interface UserDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  panNumber: string;
  role: number;
  roleName: UserRole;
  userType: number;
  userTypeName: UserType;
  approvalStatus: number;
  statusName: ApprovalStatus;
  isActive: boolean;
  createdAt: string;
  approvedAt: string | null;
  lastLoginAt: string | null;
  rejectionReason: string | null;
}

export interface RejectUserDto {
  reason?: string;
}

export interface UpdateRoleDto {
  newRole: number;
}
