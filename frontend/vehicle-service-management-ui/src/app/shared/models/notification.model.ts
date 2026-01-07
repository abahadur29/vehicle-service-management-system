/**
 * Notification model matching backend ServiceChangeHistory entity
 */
export interface Notification {
  id: number;
  serviceRequestId: number;
  targetUserId: string;
  action: string;
  message: string;
  fieldName: string;
  oldValue: string;
  newValue: string;
  changedOn: string; 
}

